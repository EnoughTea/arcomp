using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Plain data class for a single archive in parsed 7z output. </summary>
    internal class SevenZipArchiveMetadata {
        /// <summary> Gets or sets archive file name. Mandatory. </summary>
        public string Name { get; set; }

        public ArchiveType Type { get; set; }

        public DateTime? LastModified { get; set; }

        public long PhysicalSize { get; set; }

        public long Size { get; set; }

        public long PackedSize { get; set; }

        public long TotalPhysicalSize { get; set; }

        public bool IsSplit => NestedArchive != null;

        /// <summary> Gets or sets the nested archive in split archive. Null for non-split archives. </summary>
        [CanBeNull]
        public SevenZipArchiveMetadata NestedArchive { get; set; }

        /// <summary> Gets or sets the archive entries in single archive. Null for split archives. </summary>
        [CanBeNull]
        public SevenZipArchiveEntryMetadata[] Entries { get; set; }

        /// <summary> Makes archive from this metadata. </summary>
        /// <returns> Created archive. </returns>
        /// <exception cref="InvalidOperationException">Can't create archive from nameless metadata.</exception>
        [NotNull]
        public Archive ToArchive() {
            Contract.Ensures(Contract.Result<Archive>() != null);

            // Archive can be completely empty (like created with 'tar cfT archive.tar /dev/null'),
            // so most of the properties will be empty, but archives always have names.
            if (string.IsNullOrEmpty(Name)) {
                throw new InvalidOperationException("Can't create archive from nameless metadata.");
            }

            SingleArchive nestedOrSingle;
            if (!IsSplit) {
                var entries = (Entries != null) ? Entries.Select(entry => entry.ToEntry()) : Enumerable.Empty<Entry>();
                nestedOrSingle = new SingleArchive(Name, Type, entries, LastModified, PhysicalSize, Size, PackedSize);
            } else {
                Debug.Assert(NestedArchive != null);
                var entries = (NestedArchive.Entries != null)
                    ? NestedArchive.Entries.Select(entry => entry.ToEntry())
                    : Enumerable.Empty<Entry>();
                nestedOrSingle = new SingleArchive(NestedArchive.Name, NestedArchive.Type, entries, LastModified,
                    NestedArchive.PhysicalSize, NestedArchive.Size, NestedArchive.PackedSize);
            }

            return IsSplit
                ? new SplitArchive(Name, nestedOrSingle, LastModified, PhysicalSize, TotalPhysicalSize)
                : (Archive)nestedOrSingle;
        }

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString() {
            string title = NestedArchive != null ? "split archive'" : "archive '";
            string entryCount = (Entries != null) ? " with " + Entries.Length + " entires" : string.Empty;
            return title + Name + "'" + entryCount;
        }

        /// <summary> Parses all archives from the 7-Zip console listing output into their metadata. </summary>
        /// <param name="severalArchivesListing">The several seven zip archives output.</param>
        /// <returns>Sequence of parsed archives metadata.</returns>
        [NotNull]
        public static IEnumerable<SevenZipArchiveMetadata> ParseMany([CanBeNull] string severalArchivesListing) {
            Contract.Ensures(Contract.Result<IEnumerable<SevenZipArchiveMetadata>>() != null);

            if (string.IsNullOrWhiteSpace(severalArchivesListing)) { yield break; }

            int currentArchiveStart = severalArchivesListing.IndexOf(SevenZipTools.ArchiveStartMark,
                StringComparison.OrdinalIgnoreCase);
            // Iterates from archive mark to archive mark, parsing archive listings between.
            while (currentArchiveStart > 0) {
                int nextArchiveStart = severalArchivesListing.IndexOf(SevenZipTools.ArchiveStartMark,
                    currentArchiveStart + SevenZipTools.ArchiveStartMark.Length, StringComparison.OrdinalIgnoreCase);
                int currentArchiveLength = (nextArchiveStart > 0)
                    ? nextArchiveStart - currentArchiveStart
                    : severalArchivesListing.Length - currentArchiveStart;
                string archiveData = severalArchivesListing.Substring(currentArchiveStart, currentArchiveLength);
                var parsedArchive = Parse(archiveData);
                if (parsedArchive != null) {
                    yield return Parse(archiveData);
                }

                currentArchiveStart = nextArchiveStart;
            }
        }

        /// <summary> Parses 7-Zip console listing output for a single archive into archive metadata. </summary>
        /// <param name="singleArchiveListing">Output after "Listing archive:" for
        ///  current archive and until another "Listing archive:" for another archive.</param>
        /// <returns>Parsed archive metadata or null, if listing doesn't contain an archive.</returns>
        [CanBeNull]
        public static SevenZipArchiveMetadata Parse([CanBeNull] string singleArchiveListing) {
            if(string.IsNullOrWhiteSpace(singleArchiveListing)) { return null; }

            var sortedOutput = SevenZipTools.ReadSingleArchiveListing(singleArchiveListing);
            if (sortedOutput == null) { return null; }  // Sorting failed, means listing does't contain an archive.

            var result = new SevenZipArchiveMetadata();
            // Process archive properties:
            ReadArchiveProperties(sortedOutput.Properties, result);
            if (sortedOutput.IsSplitArchive) {
                Debug.Assert(sortedOutput.NestedArchiveProperties != null);
                result.NestedArchive = new SevenZipArchiveMetadata();
                ReadArchiveProperties(sortedOutput.NestedArchiveProperties, result.NestedArchive);
            }

            // Process entries:
            var singleArchive = sortedOutput.IsSplitArchive ? result.NestedArchive : result;
            Debug.Assert(singleArchive != null, "singleArchive != null");
            if (sortedOutput.IsSimpleFormat) {
                singleArchive.Entries = sortedOutput.SimpleEntries.Select(SevenZipArchiveEntryMetadata.Parse)
                    .Where(entry => entry != null).ToArray();
            } else {
                singleArchive.Entries = sortedOutput.ComplexEntries.Select(SevenZipArchiveEntryMetadata.Parse)
                    .Where(entry => entry != null).ToArray();
            }

            // 'D' attribute does not exist sometimes, like in PE archives, so try to detect directory entries.
            // For every entry a simple lookup is made to see if some other entry uses it as a directory.
            HashSet<string> knownDirectories = new HashSet<string>();
            foreach (var parentDirectory in singleArchive.Entries
                .Select(entry => Path.GetDirectoryName(entry.Name))
                .Where(parentDirectory => !string.IsNullOrEmpty(parentDirectory))) {
                knownDirectories.Add(parentDirectory);
            }
            // I think it is possible to do the same lookup in one pass, but it is not performance critical in the
            // slightest.
            foreach (var entry in singleArchive.Entries
                .Where(entryAgain => knownDirectories.Contains(entryAgain.Name) && !entryAgain.IsDirectory)) {
                entry.Attributes = entry.Attributes + "D";
            }

            return result;
        }

        private static void ReadArchiveProperties(Dictionary<string, string> properties,
            SevenZipArchiveMetadata metadata) {
            Contract.Requires(properties != null);
            Contract.Requires(metadata != null);

            metadata.Name = properties.GetValue("Path");
            if (string.IsNullOrEmpty(metadata.Name)) {
                throw new ArgumentException("Archive metadata doesn't contain path", nameof(properties));
            }

            string size = properties.GetValue("Size");
            string physicalSize = properties.GetValue("Physical Size");
            string totalPhysicalSize = properties.GetValue("Total Physical Size");
            metadata.Type = Archive.StringToType(properties.GetValue("Type"));
            metadata.Size = SevenZipTools.LongFromString(size);
            metadata.PhysicalSize = SevenZipTools.LongFromString(physicalSize);
            metadata.TotalPhysicalSize = SevenZipTools.LongFromString(totalPhysicalSize);
        }
    }
}

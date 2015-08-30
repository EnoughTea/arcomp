using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

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
        public SevenZipArchiveMetadata NestedArchive { get; set; }

        /// <summary> Gets or sets the archive entries in single archive. Null for split archives. </summary>
        public SevenZipArchiveEntryMetadata[] Entries { get; set; }

        /// <summary> Makes archive from this metadata. </summary>
        /// <returns> Created archive. </returns>
        public Archive ToArchive() {
            // Archive can be completely empty (like created with 'tar cfT archive.tar /dev/null'),
            // so most of the properties will be empty, but archives always have names.
            if (String.IsNullOrEmpty(Name)) {
                throw new InvalidOperationException("Can't create archive from nameless metadata.");
            }

            var singleNestedOrMain = !IsSplit
                ? new SingleArchive(Name, Type, Entries.Select(entry => entry.ToEntry()), LastModified,
                    PhysicalSize, Size, PackedSize)
                : new SingleArchive(NestedArchive.Name, NestedArchive.Type,
                    NestedArchive.Entries.Select(entry => entry.ToEntry()), LastModified,
                    NestedArchive.PhysicalSize, NestedArchive.Size, NestedArchive.PackedSize);

            return IsSplit
                ? new SplitArchive(Name, singleNestedOrMain, LastModified, PhysicalSize, TotalPhysicalSize)
                : (Archive)singleNestedOrMain;
        }

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString() {
            string title = NestedArchive != null ? "split archive'" : "archive '";
            return title + Name + "' with " + Entries.Length + " entires";
        }

        /// <summary> Parses all archives from the 7-Zip console listing output into their metadata. </summary>
        /// <param name="severalArchivesListing">The several seven zip archives output.</param>
        /// <returns>Sequence of parsed archives metadata.</returns>
        public static IEnumerable<SevenZipArchiveMetadata> ParseMany(string severalArchivesListing) {
            if (String.IsNullOrWhiteSpace(severalArchivesListing)) { yield break; }

            int arcStart = severalArchivesListing.IndexOf(SevenZipTools.ArchiveStartMark, StringComparison.OrdinalIgnoreCase);
            while (arcStart > 0) {
                int nextArc = severalArchivesListing.IndexOf(SevenZipTools.ArchiveStartMark,
                    arcStart + SevenZipTools.ArchiveStartMark.Length, StringComparison.OrdinalIgnoreCase);

                int arcLength = nextArc > 0 ? nextArc - arcStart : severalArchivesListing.Length - arcStart;
                string archiveData = severalArchivesListing.Substring(arcStart, arcLength);
                var parsedArchive = Parse(archiveData);
                if (parsedArchive != null) {
                    yield return Parse(archiveData);
                }

                arcStart = nextArc;
            }
        }

        /// <summary> Parses 7-Zip console listing output for a single archive into archive metadata. </summary>
        /// <param name="singleArchiveListing">Output after "Listing archive:" for
        ///  current archive and until another "Listing archive:" for another archive.</param>
        /// <returns>Parsed archive metadata or null, if listing doesn't contain an archive.</returns>
        public static SevenZipArchiveMetadata Parse(string singleArchiveListing) {
            Contract.Requires(!String.IsNullOrEmpty(singleArchiveListing));

            var sortedOutput = SevenZipTools.ReadSingleArchiveListing(singleArchiveListing);
            if (sortedOutput == null) { return null; }

            var result = new SevenZipArchiveMetadata();
            // Process archive properties:
            ReadArchiveProperties(sortedOutput.Properties, result);
            if (sortedOutput.IsSplitArchive) {
                result.NestedArchive = new SevenZipArchiveMetadata();
                ReadArchiveProperties(sortedOutput.NestedArchiveProperties, result.NestedArchive);
            }

            // Process entries:
            var singleArchive = sortedOutput.IsSplitArchive ? result.NestedArchive : result;
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
                .Where(parentDirectory => !String.IsNullOrEmpty(parentDirectory))) {
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

        private static void ReadArchiveProperties(Dictionary<string, string> metadataValues,
            SevenZipArchiveMetadata metadata) {
            Contract.Requires(metadataValues != null);
            Contract.Requires(metadata != null);

            metadata.Name = metadataValues.GetValue("Path");
            if (String.IsNullOrEmpty(metadata.Name)) {
                throw new ArgumentException("Archive metadata doesn't contain path", nameof(metadataValues));
            }

            string size = metadataValues.GetValue("Size");
            string physicalSize = metadataValues.GetValue("Physical Size");
            string totalPhysicalSize = metadataValues.GetValue("Total Physical Size");
            metadata.Type = Archive.StringToType(metadataValues.GetValue("Type"));
            metadata.Size = SevenZipTools.LongFromString(size);
            metadata.PhysicalSize = SevenZipTools.LongFromString(physicalSize);
            metadata.TotalPhysicalSize = SevenZipTools.LongFromString(totalPhysicalSize);
        }
    }
}

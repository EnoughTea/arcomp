using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Contains methods for parsing archives and entries from 7-Zip console output. </summary>
    internal static class SevenZipParser {
        /// <summary> Parses all archives from the 7-Zip console listing output into their metadata. </summary>
        /// <param name="severalArchivesListing">The several seven zip archives output.</param>
        /// <returns>Sequence of parsed archives metadata.</returns>
        [NotNull]
        public static IEnumerable<Archive> ParseArchives([CanBeNull] string severalArchivesListing) {
            Contract.Ensures(Contract.Result<IEnumerable<Archive>>() != null);

            if (string.IsNullOrWhiteSpace(severalArchivesListing)) { yield break; }

            int currentArchiveStart = severalArchivesListing.IndexOf(ArchiveStartMark,
                StringComparison.OrdinalIgnoreCase);
            // Iterates from archive mark to archive mark, parsing archive listings between.
            while (currentArchiveStart > 0) {
                int nextArchiveStart = severalArchivesListing.IndexOf(ArchiveStartMark,
                    currentArchiveStart + ArchiveStartMark.Length, StringComparison.OrdinalIgnoreCase);
                int currentArchiveLength = (nextArchiveStart > 0)
                    ? nextArchiveStart - currentArchiveStart
                    : severalArchivesListing.Length - currentArchiveStart;
                string archiveData = severalArchivesListing.Substring(currentArchiveStart, currentArchiveLength);
                var parsedArchive = ParseArchive(archiveData);
                if (parsedArchive != null) {
                    yield return parsedArchive;
                }

                currentArchiveStart = nextArchiveStart;
            }
        }

        /// <summary> Parses 7-Zip console listing output for a single archive into archive metadata. </summary>
        /// <param name="singleArchiveListing">Output after "Listing archive:" for
        ///  current archive and until another "Listing archive:" for another archive.</param>
        /// <returns>Parsed archive metadata or null, if listing doesn't contain an archive.</returns>
        [CanBeNull]
        public static Archive ParseArchive([CanBeNull] string singleArchiveListing) {
            if (string.IsNullOrWhiteSpace(singleArchiveListing)) { return null; }

            var sorted = ReadSingleArchiveListing(singleArchiveListing);
            if (sorted == null) { return null; }  // Sorting failed, means listing does't contain an archive.

            // Create archive entries first.
            var entries = sorted.IsSimpleFormat
                ? sorted.SimpleEntries.Select(ParseEntry).Where(entry => entry != null).ToArray()
                : sorted.ComplexEntries.Select(ParseEntry).Where(entry => entry != null).ToArray();

            // 'D' attribute does not exist sometimes, like in PE archives, so try to detect folder entries.
            // For every entry a simple lookup is made to see if some other entry uses it as a directory.
            var detectedDirectories = new HashSet<string>();
            foreach (var parentDirectory in entries
                .Select(entry => Path.GetDirectoryName(entry.Name))
                .Where(parentDirectory => !string.IsNullOrEmpty(parentDirectory))) {
                detectedDirectories.Add(parentDirectory);
            }
            // Now check created entries agains detected folders,
            // and fix those created file entries that should be folder entries.
            for (int index = 0; index < entries.Length; index++) {
                var entry = entries[index];
                if (detectedDirectories.Contains(entry.Name) && !(entry is FolderEntry)) {
                    entries[index] = new FolderEntry(entry.Name, entry.LastModified, entry.Size, entry.PackedSize,
                        entry.ParentFolder);
                }
            }

            // Now create archive containing these entries:
            Archive result;
            Debug.Assert(sorted.Properties != null);
            if (sorted.IsSplitArchive) {
                Debug.Assert(sorted.NestedProperties != null);
                var nestedArchive = new SingleArchive(sorted.NestedProperties.Name, sorted.NestedProperties.Type,
                    entries, null, sorted.NestedProperties.PhysicalSize, sorted.NestedProperties.Size);
                result = new SplitArchive(sorted.Properties.Name, nestedArchive, null,
                    sorted.Properties.PhysicalSize, sorted.Properties.TotalPhysicalSize);
            } else {
                result = new SingleArchive(sorted.Properties.Name, sorted.Properties.Type, entries,
                    null, sorted.Properties.PhysicalSize, sorted.Properties.Size);
            }

            return result;
        }

        #region Entry parsing

        /// <summary> Parses the specified 7-Zip output line for a single archive entry into entrymet metadata. </summary>
        /// <param name="sevenZipEntryOutput">7-Zip output for a single entry, can be simple or complex.
        ///  Simple looks like this: 2003-07-12 00:37:08 ....A 183851 26891 BlahBlah.txt"
        /// Complex consist of several lines that follow archive data pattern: Variable Name = Some value.</param>
        /// <returns>Parsed 7-Zip metadata for a single archive entry or null, if entry is nameless.</returns>
        /// <exception cref="ArgumentException">
        /// Unknown date format in 7-Zip entry.
        /// or
        /// Unknown time format in 7-Zip entry.
        /// or
        /// Unknown attributes format in 7-Zip entry.
        /// or
        /// Unknown size format in 7-Zip entry.
        /// or
        /// Unknown packed size format in 7-Zip entry.
        /// </exception>
        [CanBeNull]
        public static Entry ParseEntry([NotNull] string sevenZipEntryOutput) {
            Contract.Requires(!string.IsNullOrWhiteSpace(sevenZipEntryOutput));

            return (sevenZipEntryOutput.IndexOf(Environment.NewLine, StringComparison.OrdinalIgnoreCase) < 0)
                ? ParseFromSimpleLineEntry(sevenZipEntryOutput)
                : ParseFromDetailedLineEntry(sevenZipEntryOutput);
        }

        [CanBeNull]
        private static Entry ParseFromSimpleLineEntry([NotNull] string simpleLineEntry) {
            Contract.Requires(!string.IsNullOrEmpty(simpleLineEntry));

            if (simpleLineEntry.StartsWith("Errors:")) { return null; }

            string name = simpleLineEntry.Substring(53).Trim();
            string attributes = simpleLineEntry.Substring(20, 5).Trim();
            string size = simpleLineEntry.Substring(26, 12).Trim();
            string packedSize = simpleLineEntry.Substring(39, 12).Trim();
            string modified = simpleLineEntry.Substring(0, 19).Trim();
            return CreateFromProperties(name, modified, attributes, size, packedSize, null);
        }

        [CanBeNull]
        private static Entry ParseFromDetailedLineEntry([NotNull] string sevenZipEntryOutput) {
            Contract.Requires(!string.IsNullOrEmpty(sevenZipEntryOutput));

            var entryData = SevenZipTools.MakeValueMap(sevenZipEntryOutput);
            return CreateFromProperties(entryData.GetValue("Path"), entryData.GetValue("Modified"),
                entryData.GetValue("Attributes"), entryData.GetValue("Size"),
                entryData.GetValue("Packed Size"), entryData.GetValue("CRC"));
        }

        [CanBeNull]
        private static Entry CreateFromProperties([CanBeNull] string name, [CanBeNull] string modified,
            [CanBeNull] string attributes, [CanBeNull] string size, [CanBeNull] string packedSize,
            [CanBeNull] string crc) {
            if (string.IsNullOrWhiteSpace(name)) { return null; }

            string attributesValue = SevenZipTools.AttributesFromString(attributes);
            DateTime? modifiedValue = SevenZipTools.DateFromString(modified);
            long sizeValue = SevenZipTools.LongFromString(size);
            long packedSizeValue = SevenZipTools.LongFromString(packedSize);
            int crcValue = !string.IsNullOrWhiteSpace(crc) ? SevenZipTools.IntFromString(crc, true) : -1;

            bool isDirectory = attributesValue.Contains("D");
            return isDirectory
                ? (Entry)new FolderEntry(name, modifiedValue, sizeValue, packedSizeValue)
                : new FileEntry(name, modifiedValue, sizeValue, packedSizeValue, crcValue);
        }

        #endregion Entry parsing

        #region Console output sorting

        /// <summary>
        ///  Reads the 7-Zip console output for listing of a single archive into more managable form.
        /// </summary>
        /// <param name="singleArchiveListing">7-Zip console output for listing of a single archive.</param>
        /// <returns>Sorted archive listing or null, if console output doesn't contain an archive.</returns>
        [CanBeNull]
        private static SevenZipArchiveListing ReadSingleArchiveListing([CanBeNull] string singleArchiveListing) {
            if (string.IsNullOrEmpty(singleArchiveListing)) { return null; }

            var listing = new SevenZipArchiveListing();
            // Remove header with "Listing archive: blah.zip" part, it is useless:
            int archiveStartIndex = singleArchiveListing.IndexOf(MetaMark, StringComparison.OrdinalIgnoreCase);
            if (archiveStartIndex < 0) {
                return null;
            }

            string entireArchiveSection = singleArchiveListing.Remove(0, archiveStartIndex + MetaMark.Length);
            int entriesStart, entriesEnd;
            // Find entry lines beginning and end, so we know where archive property section ends.
            // If there are no entries, everything in archive string is archive properties.
            EntriesFormat entriesState = FindEntriesRange(entireArchiveSection, out entriesStart, out entriesEnd);
            // Now we can use found entry lines start position to find a properties section:
            string properties;
            string entries = string.Empty;
            if (entriesState == EntriesFormat.NoEntries) {
                // There are no entries, so everything is properties.
                properties = entireArchiveSection.Trim();
            } else {
                // There are entries, so everything before them is properties.
                const int simpleHeaderSize = 57;
                int entriesMarkLength = (entriesState == EntriesFormat.Simple)
                    ? EntriesMark.Length + simpleHeaderSize
                    : ComplexEntriesMark.Length;
                properties = entireArchiveSection.Substring(0, entriesStart - entriesMarkLength).Trim();
                entries = entireArchiveSection.Substring(entriesStart, entriesEnd - entriesStart);
                entries = entries.Replace("\t", " ".Repeat(4));
            }

            // Cut properties of the split archive from property string, if needed:
            int nestedStart = properties.IndexOf(MetaMark, StringComparison.OrdinalIgnoreCase);
            if (nestedStart >= 0) {
                // Mark means that it is a split archive.
                nestedStart += MetaMark.Length;
                var nestedPropertiesMap = SevenZipTools.MakeValueMap(properties.Substring(nestedStart));
                listing.NestedProperties = SevenZipArchiveListing.PropertiesFromMap(nestedPropertiesMap);
                int splitDataEnd = properties.IndexOf(SplitMark, StringComparison.OrdinalIgnoreCase);
                if (splitDataEnd > 0) {
                    properties = properties.Substring(0, splitDataEnd);
                }
            }
            // Now property string can't contain split archive properties, create property map for single archive.
            var propertiesMap = SevenZipTools.MakeValueMap(properties);
            listing.Properties = SevenZipArchiveListing.PropertiesFromMap(propertiesMap);
            // Split entries for convenience:
            if (entriesState == EntriesFormat.Simple) {
                listing.SimpleEntries = entries.Split(SevenZipTools.NewLine, StringSplitOptions.RemoveEmptyEntries);
            } else if (entriesState == EntriesFormat.Complex) {
                listing.ComplexEntries = entries.Split(SevenZipTools.EmptyLine, StringSplitOptions.RemoveEmptyEntries);
            }

            return listing;
        }

        private enum EntriesFormat { NoEntries, Simple, Complex };

        private static EntriesFormat FindEntriesRange(string entireArchiveSection, out int entriesStart,
            out int entriesEnd) {
            if (String.IsNullOrEmpty(entireArchiveSection)) {
                entriesStart = -1;
                entriesEnd = -1;
                return EntriesFormat.NoEntries;
            }

            EntriesFormat entriesState = EntriesFormat.NoEntries;
            // If neither simple nor complex format checks succeed, that means no entries.
            // Lets try to find simple entry lines first:
            entriesStart = entireArchiveSection.IndexOf(EntriesMark, StringComparison.OrdinalIgnoreCase);
            int entriesEndSearchIndex = entriesStart + EntriesMark.Length;
            if (entriesEndSearchIndex > entireArchiveSection.Length - 1) {
                entriesEndSearchIndex = entireArchiveSection.Length - 1;
            }

            entriesEnd = (entriesStart >= 0)
                ? entireArchiveSection.IndexOf(EntriesMark, entriesEndSearchIndex, StringComparison.OrdinalIgnoreCase)
                : -1;
            // Check if simple entry line format was found; if not, it could be complex format or empty archive.
            if (entriesStart < 0 || entriesEnd < 0) {
                // No simple format, so check for complex format.
                entriesStart = entireArchiveSection.IndexOf(ComplexEntriesMark, StringComparison.OrdinalIgnoreCase);
                if (entriesStart >= 0) {
                    // Entries are in complex format.
                    entriesState = EntriesFormat.Complex;
                    entriesStart += ComplexEntriesMark.Length;
                    entriesEnd = entireArchiveSection.Length;
                }
            } else {
                // Entries are in simple format.
                entriesState = EntriesFormat.Simple;
                entriesStart += EntriesMark.Length;
            }

            return entriesState;
        }

        #endregion Console output sorting

        /// <summary> Pattern used to detect start of the archive.</summary>
        private static readonly string ArchiveStartMark = Environment.NewLine + "Listing archive: ";

        /// <summary> Pattern used to detect simple archive entry section.</summary>
        private static readonly string EntriesMark = Environment.NewLine +
            "------------------- ----- ------------ ------------  ------------------------" + Environment.NewLine;
        /// <summary> Pattern used to detect complex archive entry section.</summary>
        private static readonly string ComplexEntriesMark = Environment.NewLine + "----------" + Environment.NewLine;

        /// <summary> Pattern used to detect archive metadata section.</summary>
        private static readonly string MetaMark = Environment.NewLine + "--" + Environment.NewLine;

        /// <summary> Pattern used to detect that archive is a split archive.</summary>
        private static readonly string SplitMark = Environment.NewLine + "----" + Environment.NewLine;
    }
}

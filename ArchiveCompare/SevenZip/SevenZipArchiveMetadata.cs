using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace ArchiveCompare {
    /// <summary> Plain data class for a single archive in parsed 7z output. </summary>
    public class SevenZipArchiveMetadata {
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
        public SevenZipArhiveEntryMetadata[] Entries { get; set; }

        /// <summary> Makes archive from this metadata. </summary>
        /// <returns> Created archive. </returns>
        public Archive ToArchive() {
            // Archive can be completely empty (like created with 'tar cfT archive.tar /dev/null'),
            // so most of metadata will be default, but archives always have names.
            if (String.IsNullOrEmpty(Name)) {
                throw new InvalidOperationException("Can't create archive from invalid metadata.");
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

        /// <summary> Parses the many. </summary>
        /// <param name="severalSevenZipArchivesOutput">The several seven zip archives output.</param>
        /// <returns></returns>
        public static IEnumerable<SevenZipArchiveMetadata> ParseMany(string severalSevenZipArchivesOutput) {
            int arcStart = severalSevenZipArchivesOutput.IndexOf(ArchiveStartMark, StringComparison.OrdinalIgnoreCase);
            while (arcStart > 0) {
                int nextArc = severalSevenZipArchivesOutput.IndexOf(ArchiveStartMark,
                    arcStart + ArchiveStartMark.Length, StringComparison.OrdinalIgnoreCase);

                int arcLength = nextArc > 0 ? nextArc - arcStart : severalSevenZipArchivesOutput.Length - arcStart;
                string archiveData = severalSevenZipArchivesOutput.Substring(arcStart, arcLength);
                yield return Parse(archiveData);
                arcStart = nextArc;
            }
        }

        /// <summary> Parses 7-Zip output for a single archive into archive metadata. </summary>
        /// <param name="singleSevenZipArchiveOutput">Output after "Listing archive:" for
        ///  current archive and until another "Listing archive:" for another archive.</param>
        /// <returns>Parsed archive metadata.</returns>
        public static SevenZipArchiveMetadata Parse(string singleSevenZipArchiveOutput) {
            Contract.Requires(!String.IsNullOrEmpty(singleSevenZipArchiveOutput));

            // First, prepare input data for parsing:
            // Remove header with "Listing archive: blah.zip" part:
            int archiveStartIndex = singleSevenZipArchiveOutput.IndexOf(MetaMark, StringComparison.OrdinalIgnoreCase);
            if (archiveStartIndex < 0) {
                throw new ArgumentException("Can't parse archive meta from given archive data.",
                    nameof(singleSevenZipArchiveOutput));
            }

            singleSevenZipArchiveOutput = singleSevenZipArchiveOutput.Remove(0, archiveStartIndex + MetaMark.Length);
            // Extract entries lines:
            int entriesStart = singleSevenZipArchiveOutput.IndexOf(EntriesMark, StringComparison.OrdinalIgnoreCase);
            int entriesEnd = singleSevenZipArchiveOutput.IndexOf(EntriesMark, entriesStart + EntriesMark.Length,
                StringComparison.OrdinalIgnoreCase);
            if (entriesStart < 0 || entriesEnd < 0) {
                throw new ArgumentException("Can't parse archive entries from given archive data.",
                    nameof(singleSevenZipArchiveOutput));
            }

            entriesStart += EntriesMark.Length;
            string entriesData = singleSevenZipArchiveOutput.Substring(entriesStart, entriesEnd - entriesStart);
            string[] entryLines = entriesData.Split(NewLine, StringSplitOptions.RemoveEmptyEntries);
            // Right after entries would be archive total line:
            string archiveTotalEntry = singleSevenZipArchiveOutput.Substring(Environment.NewLine, entriesEnd + EntriesMark.Length);
            var archiveTotal = SevenZipArhiveEntryMetadata.Parse(archiveTotalEntry);
            // Everything before entries can contain metadata for single or split archive:
            string archiveMeta = singleSevenZipArchiveOutput.Substring(0, entriesStart);

            // Data preparation complete, start actual parsing:
            var result = new SevenZipArchiveMetadata();
            SevenZipArchiveMetadata entriesContainer;   // Points to the archive containing entries.
                                                        // First read metadata:
            int nestedStart = archiveMeta.IndexOf(MetaMark, StringComparison.OrdinalIgnoreCase);
            if (nestedStart < 0) {
                // Parse single archive:
                int dataEnd = archiveMeta.IndexOf(EmptyLine, StringComparison.OrdinalIgnoreCase);
                string dataSection = archiveMeta.Substring(0, dataEnd);
                ReadArchiveDataPairs(dataSection, result);
                result.LastModified = archiveTotal.LastModified;
                result.Size = archiveTotal.Size;
                result.PackedSize = archiveTotal.PackedSize;
                entriesContainer = result;
            } else {
                // Parse split archive:
                nestedStart += MetaMark.Length;
                int splitDataEnd = archiveMeta.IndexOf(SplitMark, StringComparison.OrdinalIgnoreCase);
                string splitData = archiveMeta.Substring(0, splitDataEnd);
                ReadArchiveDataPairs(splitData, result);
                // Parse nested archive of the split archive:
                int nestedDataEnd = archiveMeta.IndexOf(EmptyLine, StringComparison.OrdinalIgnoreCase);
                string nestedData = archiveMeta.Substring(nestedStart, nestedDataEnd - nestedStart);
                result.NestedArchive = new SevenZipArchiveMetadata();
                ReadArchiveDataPairs(nestedData, result.NestedArchive);
                result.NestedArchive.LastModified = archiveTotal.LastModified;
                result.NestedArchive.Size = archiveTotal.Size;
                result.NestedArchive.PackedSize = archiveTotal.PackedSize;
                entriesContainer = result.NestedArchive;
            }

            // Populate entries either in a single archive, or in nested archive withing split archive:
            entriesContainer.Entries = new SevenZipArhiveEntryMetadata[entryLines.Length];
            for (int index = 0; index < entryLines.Length; index++) {
                entriesContainer.Entries[index] = SevenZipArhiveEntryMetadata.Parse(entryLines[index]);
            }

            return result;
        }

        /// <summary> Pattern used to detect start of the archive.</summary>
        private static readonly string ArchiveStartMark = Environment.NewLine + "Listing archive: ";

        /// <summary> Pattern used to detect archive entry section.</summary>
        private static readonly string EntriesMark = Environment.NewLine +
            "------------------- ----- ------------ ------------  ------------------------" + Environment.NewLine;

        /// <summary> Pattern used to detect archive metadata section.</summary>
        private static readonly string MetaMark = Environment.NewLine + "--" + Environment.NewLine;

        /// <summary> Pattern used to detect that archive is a split archive.</summary>
        private static readonly string SplitMark = Environment.NewLine + "----" + Environment.NewLine;

        /// <summary> Pattern used to split name and value in the archive metadata section.</summary>
        private const string DataEqualsMark = " = ";

        private static readonly string[] NewLine = { Environment.NewLine };
        private static readonly string EmptyLine = Environment.NewLine + Environment.NewLine;

        // Parse 7zip archive data section. Passed data section looks like:
        // Path = archive.zip
        // Type = zip
        // Some Name = Some value ...
        private static void ReadArchiveDataPairs(string archiveDataSection, SevenZipArchiveMetadata metadata) {
            var dataLines = archiveDataSection.Split(NewLine, StringSplitOptions.RemoveEmptyEntries);
            foreach (var dataLine in dataLines) {
                int index = dataLine.IndexOf(DataEqualsMark, StringComparison.OrdinalIgnoreCase);
                if (index < 0) { continue; }

                string dataVar = dataLine.Substring(0, index).Trim();
                string dataValue = dataLine.Substring(index + DataEqualsMark.Length).Trim();
                if (dataVar == "Path") { metadata.Name = dataValue; }
                if (dataVar == "Type") { metadata.Type = Archive.StringToType(dataValue); }
                if (dataVar == "Size") { metadata.Size = dataValue.ToInt64(); }
                if (dataVar == "Physical Size") { metadata.PhysicalSize = dataValue.ToInt64(); }
                if (dataVar == "Total Physical Size") { metadata.TotalPhysicalSize = dataValue.ToInt64(); }
            }
        }
    }
}

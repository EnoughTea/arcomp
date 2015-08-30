using System;
using System.Diagnostics.Contracts;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Plain data class for a single archive entry in parsed 7z output. </summary>
    internal class SevenZipArchiveEntryMetadata {
        /// <summary> Gets or sets archive entry name. Mandatory. </summary>
        public string Name { get; set; }

        public DateTime? LastModified { get; set; }

        public long Size { get; set; }

        public long PackedSize { get; set; }

        public string Attributes { get; set; }

        public bool IsDirectory => Attributes != null && Attributes.Contains("D");

        /// <summary> Makes entry from this metadata. </summary>
        /// <returns> Created entry.</returns>
        /// <exception cref="InvalidOperationException">Can't create entry from invalid metadata.</exception>
        public Entry ToEntry() {
            // Entry can be completely empty, but it will always have name.
            if (String.IsNullOrEmpty(Name)) {
                throw new InvalidOperationException("Can't create entry from invalid metadata.");
            }

            return IsDirectory
                ? (Entry)new FolderEntry(Name, null, LastModified, Size, PackedSize)
                : new FileEntry(Name, null, LastModified, Size, PackedSize);
        }

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString() {
            string title = IsDirectory ? "directory '" : "file '";
            return title + Name + "'";
        }

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
        public static SevenZipArchiveEntryMetadata Parse([NotNull] string sevenZipEntryOutput) {
            Contract.Requires(!String.IsNullOrWhiteSpace(sevenZipEntryOutput));

            return (sevenZipEntryOutput.IndexOf(Environment.NewLine, StringComparison.OrdinalIgnoreCase) < 0)
                ? ParseFromSimpleLineEntry(sevenZipEntryOutput)
                : ParseFromDetailedLineEntry(sevenZipEntryOutput);
        }

        [CanBeNull]
        private static SevenZipArchiveEntryMetadata ParseFromSimpleLineEntry([NotNull] string simpleLineEntry) {
            Contract.Requires(!String.IsNullOrEmpty(simpleLineEntry));

            if (simpleLineEntry.StartsWith("Errors:")) { return null; }

            string name = simpleLineEntry.Substring(53).Trim();
            if (String.IsNullOrWhiteSpace(name)) { return null; }

            string attributes = simpleLineEntry.Substring(20, 5).Trim();
            string size = simpleLineEntry.Substring(26, 12).Trim();
            string packedSize = simpleLineEntry.Substring(39, 12).Trim();
            string modified = simpleLineEntry.Substring(0, 19);
            return new SevenZipArchiveEntryMetadata {
                Name = name,
                Attributes = SevenZipTools.AttributesFromString(attributes),
                LastModified = SevenZipTools.DateFromString(modified),
                Size = SevenZipTools.LongFromString(size),
                PackedSize = SevenZipTools.LongFromString(packedSize)
            };
        }

        [CanBeNull]
        private static SevenZipArchiveEntryMetadata ParseFromDetailedLineEntry([NotNull] string sevenZipEntryOutput) {
            Contract.Requires(!String.IsNullOrEmpty(sevenZipEntryOutput));

            var entryData = SevenZipTools.MakeValueMap(sevenZipEntryOutput);
            string name = entryData.GetValue("Path");
            if (String.IsNullOrWhiteSpace(name)) { return null; }

            string attributes = entryData.GetValue("Attributes");
            string size = entryData.GetValue("Size");
            string packedSize = entryData.GetValue("Packed Size");
            string modified = entryData.GetValue("Modified");
            var result = new SevenZipArchiveEntryMetadata {
                Name = name,
                Attributes = SevenZipTools.AttributesFromString(attributes),
                LastModified = SevenZipTools.DateFromString(modified),
                Size = SevenZipTools.LongFromString(size),
                PackedSize = SevenZipTools.LongFromString(packedSize)
            };

            return result;
        }
    }
}

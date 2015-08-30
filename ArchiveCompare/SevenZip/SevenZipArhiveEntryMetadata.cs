using System;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;

namespace ArchiveCompare {
    /// <summary> Plain data class for a single archive entry in parsed 7z output. </summary>
    public class SevenZipArhiveEntryMetadata {
        /// <summary> Gets or sets archive entry name. Mandatory. </summary>
        public string Name { get; set; }

        public DateTime? LastModified { get; set; }

        public long Size { get; set; }

        public long PackedSize { get; set; }

        public string Attributes { get; set; }

        public bool IsDirectory => Attributes.Contains("D");

        /// <summary> Makes entry from this metadata. </summary>
        /// <returns> Created entry.</returns>
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

        /// <summary> Parses the specified 7-Zip output line for a single archive entry into entry metadata. </summary>
        /// <param name="sevenZipEntryLine">7-Zip output line for a single entry, looks like this:
        ///  2003-07-12 00:37:08 ....A 183851 26891 BlahBlah.txt"</param>
        /// <returns>Parsed 7-Zip metadata for a single archive entry.</returns>
        /// <exception cref="ArgumentException">
        /// Unknown attributes format in archive entry.
        /// or
        /// Unknown size format in archive entry.
        /// or
        /// Unknown size format in archive entry.
        /// </exception>
        public static SevenZipArhiveEntryMetadata Parse(string sevenZipEntryLine) {
            Contract.Requires(!String.IsNullOrEmpty(sevenZipEntryLine));

            string attributes = sevenZipEntryLine.Substring(20, 5).Trim();
            if (attributes != String.Empty && !AttributesChecker.IsMatch(attributes)) {
                throw new ArgumentException("Unknown attributes format in archive entry.", nameof(sevenZipEntryLine));
            }

            string size = sevenZipEntryLine.Substring(26, 12).Trim();
            if (size != String.Empty && !IntegerChecker.IsMatch(size)) {
                throw new ArgumentException("Unknown size format in archive entry.", nameof(sevenZipEntryLine));
            }

            string packedSize = sevenZipEntryLine.Substring(39, 12).Trim();
            if (packedSize != String.Empty && !IntegerChecker.IsMatch(packedSize)) {
                throw new ArgumentException("Unknown size format in archive entry.", nameof(sevenZipEntryLine));
            }

            return new SevenZipArhiveEntryMetadata {
                LastModified = LastModifiedFromEntryLine(sevenZipEntryLine),
                Attributes = attributes,
                Size = size.ToInt64(),
                PackedSize = packedSize.ToInt64(),
                Name = sevenZipEntryLine.Substring(53)
            };
        }

        private const RegexOptions StandardOptions = RegexOptions.CultureInvariant;
        private static readonly Regex DateChecker = new Regex(@"^\d+-\d+-\d+$", StandardOptions);
        private static readonly Regex TimeChecker = new Regex(@"^\d+:\d+:\d+$", StandardOptions);
        private static readonly Regex AttributesChecker = new Regex(@"^[DRHASIL\.]{5}$", StandardOptions);
        private static readonly Regex IntegerChecker = new Regex(@"^\d+$", StandardOptions);

        private static DateTime? LastModifiedFromEntryLine(string entryLine) {
            Contract.Requires(!String.IsNullOrEmpty(entryLine));

            DateTime? result = null;
            string date = entryLine.Substring(0, 10).Trim();
            string time = entryLine.Substring(11, 8).Trim();
            if (date != String.Empty) {
                if (!DateChecker.IsMatch(date)) {
                    throw new ArgumentException("Unknown date format in archive entry.", nameof(entryLine));
                }

                int year = date.Substring(0, 4).ToInt32();
                int month = date.Substring(5, 2).ToInt32();
                int day = date.Substring(8, 2).ToInt32();
                if (time != String.Empty) {
                    if (!TimeChecker.IsMatch(time)) {
                        throw new ArgumentException("Unknown time format in archive entry.", nameof(entryLine));
                    }

                    int hour = time.Substring(0, 2).ToInt32();
                    int minute = time.Substring(3, 2).ToInt32();
                    int second = time.Substring(6, 2).ToInt32();
                    result = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);
                } else {
                    result = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Local);
                }
            }

            return result;
        }
    }
}

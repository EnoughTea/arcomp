using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ArchiveCompare {
    /// <summary> Helper methods for 7-Zip related stuff. </summary>
    internal static class SevenZipTools {
        /// <summary>
        ///  Reads the 7-Zip console output for listing of a single archive into more managable form.
        /// </summary>
        /// <param name="singleArchiveListing">7-Zip console output for listing of a single archive.</param>
        /// <returns>Sorted archive listing or null, if console output doesn't contain an archive.</returns>
        public static SevenZipArchiveListing ReadSingleArchiveListing(string singleArchiveListing) {
            var listing = new SevenZipArchiveListing();

            // Remove header with "Listing archive: blah.zip" part, it is useless:
            int archiveStartIndex = singleArchiveListing.IndexOf(MetaMark, StringComparison.OrdinalIgnoreCase);
            if (archiveStartIndex < 0) {
                return null;
            }

            string entireArchiveSection = singleArchiveListing.Remove(0, archiveStartIndex + MetaMark.Length);
            bool noEntries;
            bool simpleFormat;
            // Find entry lines beginning and end, so we know where properties end.
            // Start with simple entry lines format first:
            int entriesStart = entireArchiveSection.IndexOf(EntriesMark, StringComparison.OrdinalIgnoreCase);
            int entriesEndSearchIndex = entriesStart + EntriesMark.Length;
            if (entriesEndSearchIndex > entireArchiveSection.Length - 1) {
                entriesEndSearchIndex = entireArchiveSection.Length - 1;
            }
            int entriesEnd = entireArchiveSection.IndexOf(EntriesMark, entriesEndSearchIndex,
                StringComparison.OrdinalIgnoreCase);
            // Check if simple entry line format was found; if not, it could be complex format or empty archive.
            if (entriesStart < 0 || entriesEnd < 0) {
                // No simple format, so check for complex format.
                simpleFormat = false;
                entriesStart = entireArchiveSection.IndexOf(ComplexEntriesMark, StringComparison.OrdinalIgnoreCase);
                if (entriesStart >= 0) {
                    // Entries are in complex format.
                    noEntries = false;
                    entriesStart += ComplexEntriesMark.Length;
                    entriesEnd = entireArchiveSection.Length;
                } else {
                    // Neither simple nor complex format, which means no entries, so everything is a metadata.
                    noEntries = true;
                }
            } else {
                // Entries are in simple format.
                simpleFormat = true;
                noEntries = false;
                entriesStart += EntriesMark.Length;
            }

            // Now we can use found entry lines start position to find a properties section:
            string properties;
            string entries;
            if (noEntries) {
                // There are no entries, so everything is properties.
                properties = entireArchiveSection.Trim();
                entries = null;
            } else {
                // There are entries, so everything before them is properties.
                const int simpleHeaderSize = 57;
                int entriesMarkLength = simpleFormat
                    ? EntriesMark.Length + simpleHeaderSize
                    : ComplexEntriesMark.Length;
                properties = entireArchiveSection.Substring(0, entriesStart - entriesMarkLength).Trim();
                entries = entireArchiveSection.Substring(entriesStart, entriesEnd - entriesStart);
                entries = entries.Replace("\t", " ".Repeat(4));
            }
            // Divide properties into split and single archives properties:
            int nestedStart = properties.IndexOf(MetaMark, StringComparison.OrdinalIgnoreCase);
            if (nestedStart >= 0) { // Mark means that it is a split archive.
                nestedStart += MetaMark.Length;
                listing.NestedArchiveProperties = MakeValueMap(properties.Substring(nestedStart));
                int splitDataEnd = properties.IndexOf(SplitMark, StringComparison.OrdinalIgnoreCase);
                listing.Properties = MakeValueMap(properties.Substring(0, splitDataEnd));
            } else {
                listing.Properties = MakeValueMap(properties);
            }
            // Split entries for convenience:
            if (!noEntries) {
                if (simpleFormat) {
                    listing.SimpleEntries = entries.Split(NewLine, StringSplitOptions.RemoveEmptyEntries);
                } else {
                    listing.ComplexEntries = entries.Split(EmptyLine, StringSplitOptions.RemoveEmptyEntries);
                }
            }

            return listing;
        }

        /// <summary> Turns 7zip data section into a key-value map. </summary>
        /// <remarks> Passed data section looks like: <code>
        /// Path = archive.zip
        /// Type = zip
        /// Some Name = Some value
        /// </code></remarks>
        /// <param name="keyValueLines">The seven zip value lines.</param>
        /// <returns></returns>
        public static Dictionary<string, string> MakeValueMap(string keyValueLines) {
            var valueMap = new Dictionary<string, string>();
            var dataLines = keyValueLines.Split(NewLine, StringSplitOptions.RemoveEmptyEntries);
            foreach (var dataLine in dataLines) {
                int index = dataLine.IndexOf(DataEqualsMark, StringComparison.OrdinalIgnoreCase);
                if (index < 0) { continue; }

                string dataVar = dataLine.Substring(0, index).Trim();
                string dataValue = dataLine.Substring(index + DataEqualsMark.Length).Trim();
                valueMap[dataVar] = dataValue;
            }

            return valueMap;
        }

        public static string AttributesFromString(string attributes) {
            if (!String.IsNullOrWhiteSpace(attributes) && !AttributesChecker.IsMatch(attributes)) {
                throw new ArgumentException("Unknown attributes format in 7-Zip entry.", nameof(attributes));
            }

            return attributes;
        }

        public static long LongFromString(string longRepr) {
            if (!String.IsNullOrWhiteSpace(longRepr) && !IntegerChecker.IsMatch(longRepr)) {
                throw new ArgumentException("Unknown number format in 7-Zip entry.", nameof(longRepr));
            }

            return longRepr.ToInt64();
        }

        /// <exception cref="ArgumentException">Unknown number format in 7-Zip entry.</exception>
        public static long IntFromString(string intRepr) {
            if (!String.IsNullOrWhiteSpace(intRepr) && !IntegerChecker.IsMatch(intRepr)) {
                throw new ArgumentException("Unknown number format in 7-Zip entry.", nameof(intRepr));
            }

            return intRepr.ToInt32();
        }

        /// <exception cref="ArgumentException">Unknown date format in 7-Zip entry
        /// or
        /// Unknown time format in 7-Zip entry.</exception>
        public static DateTime? DateFromString(string dateTimeRepr) {
            DateTime? dateTime = null;
            if (String.IsNullOrWhiteSpace(dateTimeRepr)) { return null; }

            string date = dateTimeRepr.Substring(0, 10).Trim();
            string time = dateTimeRepr.Substring(11, 8).Trim();
            if (date != String.Empty) {
                if (!DateChecker.IsMatch(date)) {
                    throw new ArgumentException("Unknown date format in 7-Zip entry.", nameof(dateTimeRepr));
                }

                int year = date.Substring(0, 4).ToInt32();
                int month = date.Substring(5, 2).ToInt32();
                int day = date.Substring(8, 2).ToInt32();
                if (time != String.Empty) {
                    if (!TimeChecker.IsMatch(time)) {
                        throw new ArgumentException("Unknown time format in 7-Zip entry.", nameof(dateTimeRepr));
                    }

                    int hour = time.Substring(0, 2).ToInt32();
                    int minute = time.Substring(3, 2).ToInt32();
                    int second = time.Substring(6, 2).ToInt32();
                    dateTime = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);
                } else {
                    dateTime = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Local);
                }
            }

            return dateTime;
        }

        /// <summary> Pattern used to detect start of the archive.</summary>
        internal static readonly string ArchiveStartMark = Environment.NewLine + "Listing archive: ";

        /// <summary> Pattern used to detect simple archive entry section.</summary>
        private static readonly string EntriesMark = Environment.NewLine +
            "------------------- ----- ------------ ------------  ------------------------" + Environment.NewLine;
        /// <summary> Pattern used to detect complex archive entry section.</summary>
        private static readonly string ComplexEntriesMark = Environment.NewLine + "----------" + Environment.NewLine;

        /// <summary> Pattern used to detect archive metadata section.</summary>
        private static readonly string MetaMark = Environment.NewLine + "--" + Environment.NewLine;

        /// <summary> Pattern used to detect that archive is a split archive.</summary>
        private static readonly string SplitMark = Environment.NewLine + "----" + Environment.NewLine;

        /// <summary> Pattern used to split name and value in the archive metadata section.</summary>
        private const string DataEqualsMark = " = ";

        private const RegexOptions StandardOptions = RegexOptions.CultureInvariant;
        private static readonly Regex DateChecker = new Regex(@"^\d+-\d+-\d+$", StandardOptions);
        private static readonly Regex TimeChecker = new Regex(@"^\d+:\d+:\d+$", StandardOptions);
        private static readonly Regex AttributesChecker = new Regex(@"^[DRHASIL\.]+$", StandardOptions);
        private static readonly Regex IntegerChecker = new Regex(@"^\d+$", StandardOptions);

        internal static readonly string[] NewLine = { Environment.NewLine };
        internal static readonly string[] EmptyLine = { Environment.NewLine + Environment.NewLine };
    }
}

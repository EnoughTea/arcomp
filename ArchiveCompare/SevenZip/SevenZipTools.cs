using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Helper methods for 7-Zip related stuff. </summary>
    internal static class SevenZipTools {
        /// <summary>
        ///  Reads the 7-Zip console output for listing of a single archive into more managable form.
        /// </summary>
        /// <param name="singleArchiveListing">7-Zip console output for listing of a single archive.</param>
        /// <returns>Sorted archive listing or null, if console output doesn't contain an archive.</returns>
        [CanBeNull]
        public static SevenZipArchiveListing ReadSingleArchiveListing([CanBeNull] string singleArchiveListing) {
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
                listing.NestedArchiveProperties = MakeValueMap(properties.Substring(nestedStart));
                int splitDataEnd = properties.IndexOf(SplitMark, StringComparison.OrdinalIgnoreCase);
                if (splitDataEnd > 0) {
                    properties = properties.Substring(0, splitDataEnd);
                }
            }
            // Now property string can't contain split archive properties, create property map for single archive.
            listing.Properties = MakeValueMap(properties);
            // Split entries for convenience:
            if (entriesState == EntriesFormat.Simple) {
                listing.SimpleEntries = entries.Split(NewLine, StringSplitOptions.RemoveEmptyEntries);
            } else if (entriesState == EntriesFormat.Complex) {
                listing.ComplexEntries = entries.Split(EmptyLine, StringSplitOptions.RemoveEmptyEntries);
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
        /// <returns> Map of value name → value pairs. </returns>
        [NotNull]
        public static Dictionary<string, string> MakeValueMap([CanBeNull] string keyValueLines) {
            var valueMap = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(keyValueLines)) { return valueMap; }
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

        /// <exception cref="ArgumentException">Unknown attributes format in 7-Zip entry.</exception>
        /// <exception cref="RegexMatchTimeoutException">A regex time-out occurred.</exception>
        public static string AttributesFromString([CanBeNull] string attributes) {
            if (!string.IsNullOrWhiteSpace(attributes) && !AttributesChecker.IsMatch(attributes)) {
                throw new ArgumentException("Unknown attributes format in 7-Zip entry.", nameof(attributes));
            }

            return attributes ?? string.Empty;
        }

        /// <exception cref="ArgumentException">Unknown number format in 7-Zip entry.</exception>
        /// <exception cref="RegexMatchTimeoutException">A regex time-out occurred.</exception>
        /// <exception cref="OverflowException"><paramref name="longRepr" /> represents a number that is less than
        /// <see cref="F:System.Int64.MinValue" /> or greater than <see cref="F:System.Int64.MaxValue" />. </exception>
        public static long LongFromString([CanBeNull] string longRepr) {
            if (!string.IsNullOrWhiteSpace(longRepr) && !IntegerChecker.IsMatch(longRepr)) {
                throw new ArgumentException("Unknown number format in 7-Zip entry.", nameof(longRepr));
            }

            return longRepr.ToInt64();
        }

        /// <exception cref="ArgumentException">Unknown number format in 7-Zip entry.</exception>
        /// <exception cref="RegexMatchTimeoutException">A regex time-out occurred.</exception>
        /// <exception cref="OverflowException"><paramref name="intRepr" /> represents a number that is less than
        /// <see cref="F:System.Int32.MinValue" /> or greater than <see cref="F:System.Int32.MaxValue" />. </exception>
        public static long IntFromString([CanBeNull] string intRepr) {
            if (!string.IsNullOrWhiteSpace(intRepr) && !IntegerChecker.IsMatch(intRepr)) {
                throw new ArgumentException("Unknown number format in 7-Zip entry.", nameof(intRepr));
            }

            return intRepr.ToInt32();
        }

        /// <exception cref="ArgumentException">Unknown date format in 7-Zip entry
        /// or
        /// Unknown time format in 7-Zip entry.</exception>
        /// <exception cref="RegexMatchTimeoutException">A regex time-out occurred.</exception>
        /// <exception cref="ArgumentOutOfRangeException">DateTime ctor fail: year is less than 1 or greater than 9999.
        /// -or- month is less than 1 or greater than 12. -or- day is less than 1 or greater than the number of days
        ///  month -or- hour is less than 0 or greater than 23. -or- minute is less than 0 or greater than 59.
        /// -or- second is less than 0 or greater than 59. </exception>
        public static DateTime? DateFromString([CanBeNull] string dateTimeRepr) {
            DateTime? dateTime = null;
            if (string.IsNullOrWhiteSpace(dateTimeRepr)) { return null; }

            string date = dateTimeRepr.Substring(0, 10).Trim();
            string time = dateTimeRepr.Substring(11, 8).Trim();
            if (date != string.Empty) {
                if (!DateChecker.IsMatch(date)) {
                    throw new ArgumentException("Unknown date format in 7-Zip entry.", nameof(dateTimeRepr));
                }

                int year = date.Substring(0, 4).ToInt32();
                int month = date.Substring(5, 2).ToInt32();
                int day = date.Substring(8, 2).ToInt32();
                if (time != string.Empty) {
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

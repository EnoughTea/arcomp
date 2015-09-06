using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Helper methods for 7-Zip related stuff. </summary>
    internal static class SevenZipTools {
        public static readonly string[] NewLine = { Environment.NewLine };

        public static readonly string[] EmptyLine = { Environment.NewLine + Environment.NewLine };

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
        public static long LongFromString([CanBeNull] string longRepr, bool hex = false) {
            if (string.IsNullOrWhiteSpace(longRepr)) { return 0; }

            long parsed;
            if (hex) {
                bool isHex = HexChecker.IsMatch(longRepr);
                if (!isHex) {
                    throw new ArgumentException("Unknown hexadecimal number format in 7-Zip entry.", nameof(longRepr));
                }

                parsed = Int64.Parse(longRepr, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

            } else {
                bool isDec = DecChecker.IsMatch(longRepr);
                if (!isDec) {
                    throw new ArgumentException("Unknown decimal number format in 7-Zip entry.", nameof(longRepr));
                }

                parsed = Int64.Parse(longRepr, NumberStyles.Integer, CultureInfo.InvariantCulture);
            }

            return parsed;
        }

        /// <exception cref="ArgumentException">Unknown number format in 7-Zip entry.</exception>
        /// <exception cref="RegexMatchTimeoutException">A regex time-out occurred.</exception>
        /// <exception cref="OverflowException"><paramref name="intRepr" /> represents a number that is less than
        /// <see cref="F:System.Int32.MinValue" /> or greater than <see cref="F:System.Int32.MaxValue" />. </exception>
        public static int IntFromString([CanBeNull] string intRepr, bool hex = false) {
            if (string.IsNullOrWhiteSpace(intRepr)) { return 0; }

            int parsed;
            if (hex) {
                bool isHex = HexChecker.IsMatch(intRepr);
                if (!isHex) {
                    throw new ArgumentException("Unknown hexadecimal number format in 7-Zip entry.", nameof(intRepr));
                }

                parsed = Int32.Parse(intRepr, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

            } else {
                bool isDec = DecChecker.IsMatch(intRepr);
                if (!isDec) {
                    throw new ArgumentException("Unknown decimal number format in 7-Zip entry.", nameof(intRepr));
                }

                parsed = Int32.Parse(intRepr, NumberStyles.Integer, CultureInfo.InvariantCulture);
            }

            return parsed;
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

        /// <summary> Pattern used to split name and value in the archive metadata section.</summary>
        private const string DataEqualsMark = " = ";

        // These are used to check 7-Zip data strings to catch possible errors early:
        private const RegexOptions StandardOptions = RegexOptions.CultureInvariant;
        private static readonly Regex DateChecker = new Regex(@"^\d+-\d+-\d+$", StandardOptions);
        private static readonly Regex TimeChecker = new Regex(@"^\d+:\d+:\d+$", StandardOptions);
        private static readonly Regex AttributesChecker = new Regex(@"^[DRHASIL\.]+$", StandardOptions);
        private static readonly Regex DecChecker = new Regex(@"^\d+$", RegexOptions.CultureInvariant);
        private static readonly Regex HexChecker = new Regex(@"^[\da-fA-F]+$", RegexOptions.CultureInvariant);
    }
}

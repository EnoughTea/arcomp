using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace ArchiveCompare {
    /// <summary> Extension methods for <see cref="string"/>. </summary>
    internal static class StringExtensions {
        /// <summary> Repeats given string for the specified amount of times. </summary>
        /// <param name="text">String to repeat.</param>
        /// <param name="count">Number of times given string must be repeated.</param>
        /// <returns>The resulting string.</returns>
        [Pure]
        public static string Repeat(this string text, int count) {
            return new StringBuilder().Insert(0, text, count).ToString();
        }

        [Pure]
        public static int IndexOfNextSymbol(this string self) {
            Contract.Requires(self != null);

            return self.Cast<char>().TakeWhile(char.IsWhiteSpace).Count();
        }

        [Pure]
        public static int IndexOfNextWhitespace(this string self) {
            Contract.Requires(self != null);

            return self.Cast<char>().TakeWhile(c => !char.IsWhiteSpace(c)).Count();
        }

        [Pure]
        public static string TakeSymbolsUntilWhitespace(this string self, int startIndex = 0) {
            Contract.Requires(self != null);
            Contract.Requires(startIndex >= 0);

            int end;
            return self.TakeSymbolsUntilWhitespace(startIndex, out end);
        }

        [Pure]
        public static string TakeSymbolsUntilWhitespace(this string self, int startIndex, out int endIndex) {
            Contract.Requires(self != null);
            Contract.Requires(startIndex >= 0);

            if (startIndex > 0) {
                self = self.Substring(startIndex);
            }

            int start = self.IndexOfNextSymbol();
            int end = self.Substring(start).IndexOfNextWhitespace();
            endIndex = start + end;
            return self.Substring(start, end);
        }

        /// <summary> Converts string representation of Int64 to actual Int64.
        ///  Treats null and whitespace strings as 0. </summary>
        /// <param name="self">String to convert.</param>
        /// <returns> Converted string. </returns>
        [Pure]
        public static long ToInt64(this string self) {
            return !String.IsNullOrWhiteSpace(self) ? Convert.ToInt64(self) : 0;
        }

        /// <summary> Converts string representation of Int32 to actual Int32.
        ///  Treats null and whitespace strings as 0. </summary>
        /// <param name="self">String to convert.</param>
        /// <returns> Converted string. </returns>
        [Pure]
        public static int ToInt32(this string self) {
            return !String.IsNullOrWhiteSpace(self) ? Convert.ToInt32(self) : 0;
        }
    }
}

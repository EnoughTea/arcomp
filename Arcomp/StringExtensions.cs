using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Arcomp {
    internal static class StringExtensions {
        /// <summary> Retrieves a substring from this instance. The substring starts at a specified
        ///  character position and continues until index of target string. </summary>
        /// <param name="self">Instance to retrieve substring from.</param>
        /// <param name="target">The target string.</param>
        /// <param name="startIndex">The zero-based starting character position of a substring in this instance.
        /// </param>
        /// <returns>A string that is that begins at startIndex in this instance and ends at target string position,
        ///  or String.Empty if startIndex is equal to the length of this instance or instance is empty .</returns>
        public static string Substring(this string self, string target, int startIndex = 0) {
            Contract.Requires(self != null);
            Contract.Requires(startIndex >= 0);

            string result = String.Empty;
            if (self != String.Empty) {
                int targetIndex = self.IndexOf(target, startIndex, StringComparison.OrdinalIgnoreCase);
                int length = targetIndex - startIndex;
                if (targetIndex >= 0 && length > 0) {
                    result = self.Substring(startIndex, length);
                }
            }

            return result;
        }

        public static string Between(this string self, string marker, int position) {
            int firstMarkerPosition = self.LastIndexOf(marker, position, StringComparison.Ordinal);
            int secondMarkerPosition = self.IndexOf(marker, position, StringComparison.Ordinal);
            if (firstMarkerPosition < 0) { firstMarkerPosition = 0; }
            if (secondMarkerPosition < 0) { secondMarkerPosition = self.Length; }

            int length = secondMarkerPosition - firstMarkerPosition;
            return length > 0 ? self.Substring(firstMarkerPosition, length) : self;
        }

        public static string Remove(this string self, string target, int startIndex = 0) {
            Contract.Requires(self != null);
            Contract.Requires(target != null);
            Contract.Requires(startIndex >= 0);

            int index = self.IndexOf(target, startIndex, StringComparison.Ordinal);
            return (index < 0 || target.Length < 1) ? self : self.Remove(index, target.Length);
        }

        public static int IndexOfNextSymbol(this string line) {
            return line.TakeWhile(char.IsWhiteSpace).Count();
        }

        public static int IndexOfNextWhitespace(this string line) {
            return line.TakeWhile(c => !char.IsWhiteSpace(c)).Count();
        }

        public static string TakeSymbolsUntilWhitespace(this string self, int startIndex = 0) {
            int end;
            return self.TakeSymbolsUntilWhitespace(startIndex, out end);
        }

        public static string TakeSymbolsUntilWhitespace(this string self, int startIndex, out int endIndex) {
            if (startIndex > 0) {
                self = self.Substring(startIndex);
            }

            int start = self.IndexOfNextSymbol();
            int end = self.Substring(start).IndexOfNextWhitespace();
            endIndex = start + end;
            return self.Substring(start, end);
        }

        /// <summary> Converts string representation of Int64 to actual Int64.
        ///  Treats null and empty strings as 0. </summary>
        /// <param name="self">String to convert.</param>
        /// <returns> Converted string. </returns>
        public static long ToInt64(this string self) {
            return !String.IsNullOrEmpty(self) ? Convert.ToInt64(self) : 0;
        }

        /// <summary> Converts string representation of Int32 to actual Int32.
        ///  Treats null and empty strings as 0. </summary>
        /// <param name="self">String to convert.</param>
        /// <returns> Converted string. </returns>
        public static int ToInt32(this string self) {
            return !String.IsNullOrEmpty(self) ? Convert.ToInt32(self) : 0;
        }
    }
}

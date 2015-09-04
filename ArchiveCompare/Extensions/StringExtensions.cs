using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Extension methods for <see cref="string"/>. </summary>
    internal static class StringExtensions {
        /// <summary> Repeats given string for the specified amount of times. </summary>
        /// <param name="text">String to repeat.</param>
        /// <param name="count">Number of times given string must be repeated.</param>
        /// <returns>The resulting string.</returns>
        [System.Diagnostics.Contracts.Pure, NotNull]
        public static string Repeat(this string text, int count) {
            Contract.Requires(text != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return (text == string.Empty)
                ? string.Empty
                : new StringBuilder(text.Length * count).Insert(0, text, count).ToString();
        }

        /// <summary> Finds index of the next non-whitespace symbol in the string. </summary>
        /// <param name="text"> Searched string. </param>
        /// <returns> Index of the next non-whitespace symbol in the string. </returns>
        /// <exception cref="OverflowException"> Index of next symbol is larger than
        ///  <see cref="F:System.Int32.MaxValue" />. </exception>
        [System.Diagnostics.Contracts.Pure]
        public static int IndexOfNextSymbol(this string text) {
            Contract.Requires(text != null);
            Contract.Ensures(Contract.Result<int>() >= 0);

            return text.Cast<char>().TakeWhile(char.IsWhiteSpace).Count();
        }

        /// <exception cref="OverflowException"> Index of next whitespace is larger than
        ///  <see cref="F:System.Int32.MaxValue" />. </exception>
        [System.Diagnostics.Contracts.Pure]
        public static int IndexOfNextWhitespace(this string text) {
            Contract.Requires(text != null);
            Contract.Ensures(Contract.Result<int>() >= 0);

            return text.Cast<char>().TakeWhile(c => !char.IsWhiteSpace(c)).Count();
        }

        /// <summary> Finds and returns all symbols from the start index until first whitespace symbol. </summary>
        /// <param name="text">Text to search.</param>
        /// <param name="startIndex">Search start index.</param>
        /// <returns>Substring from first symbol since start index until first whitespace.</returns>
        /// <exception cref="OverflowException"> Next whitespace is at index larger than
        ///  <see cref="F:System.Int32.MaxValue" />. </exception>
        [System.Diagnostics.Contracts.Pure, NotNull]
        public static string TakeSymbolsUntilWhitespace(this string text, int startIndex = 0) {
            Contract.Requires(text != null);
            Contract.Requires(startIndex >= 0);
            Contract.Ensures(Contract.Result<string>() != null);

            int end;
            return text.TakeSymbolsUntilWhitespace(startIndex, out end);
        }

        /// <summary> Finds and returns all symbols from the start index until first whitespace symbol. </summary>
        /// <param name="text">Text to search.</param>
        /// <param name="startIndex">Search start index.</param>
        /// <param name="endIndex">Index where whitespace was found.</param>
        /// <returns>Substring from first symbol since start index until first whitespace.</returns>
        /// <exception cref="OverflowException">Next whitespace is at index larger than
        /// <see cref="F:System.Int32.MaxValue" />.</exception>
        [System.Diagnostics.Contracts.Pure, NotNull]
        public static string TakeSymbolsUntilWhitespace(this string text, int startIndex, out int endIndex) {
            Contract.Requires(text != null);
            Contract.Requires(startIndex >= 0);
            Contract.Ensures(Contract.Result<string>() != null);

            if (text == string.Empty) {
                endIndex = 0;
                return string.Empty;
            }

            if (startIndex > 0) {
                text = text.Substring(startIndex);
            }

            int start = text.IndexOfNextSymbol();
            int end = text.Substring(start).IndexOfNextWhitespace();
            endIndex = start + end;
            return text.Substring(start, end);
        }

        /// <summary> Converts string representation of decimal integer to an actual integer.
        ///  Treats null and whitespace strings as 0. </summary>
        /// <param name="intRepr">String to convert.</param>
        /// <returns> Converted string. </returns>
        /// <exception cref="FormatException"><paramref name="intRepr" /> does not consist of an optional sign
        ///  followed by a sequence of digits (0 through 9), and is not a null or whitespace string. </exception>
        /// <exception cref="OverflowException"><paramref name="intRepr" /> represents a number that is less than
        /// <see cref="F:System.Int32.MinValue" /> or greater than <see cref="F:System.Int32.MaxValue" />. </exception>
        [System.Diagnostics.Contracts.Pure]
        public static int ToInt32([CanBeNull] this string intRepr) {
            // This extension method can be called on null string by design.
            if (string.IsNullOrWhiteSpace(intRepr)) { return 0; }

            return !string.IsNullOrWhiteSpace(intRepr)
                ? Int32.Parse(intRepr, NumberStyles.Number, CultureInfo.InvariantCulture)
                : 0;
        }
    }
}

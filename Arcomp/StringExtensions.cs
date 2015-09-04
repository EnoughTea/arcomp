using System.Diagnostics.Contracts;
using System.Text;

namespace Arcomp {
    internal static class StringExtensions {
        /// <summary> Repeats given string for the specified amount of times. </summary>
        /// <param name="text">String to repeat.</param>
        /// <param name="count">Number of times given string must be repeated.</param>
        /// <returns>The resulting string.</returns>
        [Pure]
        public static string Repeat(this string text, int count) {
            Contract.Requires(text != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return (text == string.Empty)
                ? string.Empty
                : new StringBuilder(text.Length * count).Insert(0, text, count).ToString();
        }
    }
}

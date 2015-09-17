using System.Collections.Generic;

namespace ArchiveCompare {
    /// <summary> Global extension methods. </summary>
    internal static class GlobalExtensions {
        /// <summary> Wraps the specified object into enumerable sequence. </summary>
        /// <typeparam name="T">Type of the wrapped object.</typeparam>
        /// <param name="target">Wrapped object.</param>
        /// <returns>Sequence consisting of the single wrapped object.</returns>
        public static IEnumerable<T> Yield<T>(this T target) {
            yield return target;
        }
    }
}

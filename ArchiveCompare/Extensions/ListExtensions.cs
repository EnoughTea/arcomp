using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace ArchiveCompare {
    /// <summary> Extension methods for <see cref="List{T}"/> </summary>
    internal static class ListExtensions {
        /// <summary> Pops the first item in the list. </summary>
        /// <typeparam name="T">Type of the list element.</typeparam>
        /// <param name="list">List to pop an element from.</param>
        /// <returns>Popped first element or default(T).</returns>
        public static T PopFirst<T>(this List<T> list) {
            Contract.Requires(list != null);

            var first = default(T);
            if (list.Count > 0) {
                first = list[0];
                list.RemoveAt(0);
            }

            return first;
        }
    }
}

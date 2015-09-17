using System.Collections.Generic;
using System.Diagnostics.Contracts;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Extension methods for <see cref="Dictionary{TKey,TValue}"/>. </summary>
    internal static class DictionaryExtensions {
        /// <summary>Returns <paramref name="defaultValue" /> if the given <paramref name="key" />
        /// is not present within the dictionary.</summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="map">The dictionary to search for value.</param>
        /// <param name="key">The key to look for.</param>
        /// <param name="defaultValue">The default value to be returned if the specified key is not present.</param>
        /// <returns>Value matching specified <paramref name="key" /> or
        /// <paramref name="defaultValue" /> if none is found.</returns>
        [System.Diagnostics.Contracts.Pure, CanBeNull]
        public static TValue GetValue<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> map, [NotNull] TKey key,
            TValue defaultValue = default(TValue)) {
            Contract.Requires(map != null);
            Contract.Requires(!ReferenceEquals(key, null));

            TValue value;
            return map.TryGetValue(key, out value) ? value : defaultValue;
        }
    }
}

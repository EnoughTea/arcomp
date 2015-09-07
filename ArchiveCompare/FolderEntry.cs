using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an archived directory. </summary>
    public class FolderEntry : Entry {
        /// <summary> Initializes a new instance of the <see cref="FileEntry"/> class. </summary>
        /// <param name="name">Full directory name.</param>
        /// <param name="lastModified">The date when folder was last modified.</param>
        /// <param name="size">Uncompressed size. 0 means uncompressed size is unavailable.</param>
        /// <param name="packedSize">Compressed size. 0 means that either compressed size is unavailable or
        ///     no compression was done on the entry.</param>
        /// <param name="parent">Parent directory. Null means entry is located at archive's root.</param>
        public FolderEntry(string name, DateTime? lastModified = null, long size = 0, long packedSize = 0,
            FolderEntry parent = null)
            : base(name, lastModified, size, packedSize, parent) {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(size >= 0);
            Contract.Requires(packedSize >= 0);

            _contents = new HashSet<Entry>();
        }

        private readonly HashSet<Entry> _contents;

        /// <summary> Gets entries which belong to this directory, top-level only. </summary>
        [NotNull]
        public IEnumerable<Entry> Contents => _contents;

        /// <summary> Gets the amount of entries in top level of this folder. </summary>
        public int ContentsCount => _contents.Count;

        /// <summary> Determines whether this entry has the specified entry as top-level child. </summary>
        /// <param name="entry">Entry to check.</param>
        /// <returns>true if the specified entry is a top-level child; false otherwise.</returns>
        public bool Contains([CanBeNull] Entry entry) {
            return entry != null && _contents.Contains(entry);
        }

        /// <summary> Gathers all entries which belong to this directory, top-level and recursively down. </summary>
        [NotNull]
        public IEnumerable<Entry> FlattenContents() {
            Contract.Ensures(Contract.Result<IEnumerable<Entry>>() != null);

            foreach (var child in Contents) {
                yield return child;
                var childFolder = (child as FolderEntry);
                if (childFolder == null) continue;

                foreach (var subChild in childFolder.FlattenContents()) {
                    yield return subChild;
                }
            }
        }

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString() {
            return "folder " + base.ToString();
        }

        internal void Add(Entry entry) {
            Contract.Requires(entry != null);

            _contents.Add(entry);
        }

        [ContractInvariantMethod]
        private void ObjectInvariant() {
            Contract.Invariant(Contents != null);
        }
    }
}
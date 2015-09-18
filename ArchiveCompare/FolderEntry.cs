using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an archived directory. </summary>
    [DataContract(Name = "folder", IsReference = true, Namespace = "")]
    public class FolderEntry : Entry {
        /// <summary> Initializes a new instance of the <see cref="FileEntry" /> class. </summary>
        /// <param name="path">Full directory name.</param>
        /// <param name="lastModified">The date when folder was last modified.</param>
        /// <param name="size">Uncompressed size. 0 means uncompressed size is unavailable.</param>
        /// <param name="packedSize">Compressed size. 0 means that either compressed size is unavailable or
        /// no compression was done on the entry.</param>
        /// <param name="parent">Parent directory. Null means entry is located at archive's root.</param>
        /// <param name="contents">Folder contents. Null means folder is empty.</param>
        public FolderEntry(string path, DateTime? lastModified = null, long size = 0, long packedSize = 0,
            FolderEntry parent = null, IEnumerable<Entry> contents = null)
            : base(path, lastModified, size, packedSize, parent) {
            Contract.Requires(!string.IsNullOrWhiteSpace(path));
            Contract.Requires(size >= 0);
            Contract.Requires(packedSize >= 0);

            _contents = new HashSet<Entry>(contents ?? Enumerable.Empty<Entry>());
        }

        [DataMember(Name = "contents", IsRequired = true, Order = 10)]
        private readonly HashSet<Entry> _contents;

        /// <summary> Gets name of this folder. </summary>
        public override string Name => Path.Split(PathSeparator, StringSplitOptions.RemoveEmptyEntries).Last();

        /// <summary> Gets entries which belong to this directory, top-level only. </summary>
        [NotNull]
        public IEnumerable<Entry> Contents => _contents;

        /// <summary> Gets the amount of files in top level of this folder. </summary>
        public int FileCount => _contents.Count(entry => entry is FileEntry);

        /// <summary> Gets the amount of folders in top level of this folder. </summary>
        public int FolderCount => _contents.Count(entry => entry is FolderEntry);

        /// <summary> Gets the amount of files and folders in top level of this folder. </summary>
        public int ContentsCount => _contents.Count;

        /// <summary> Determines whether this entry has the specified entry as top-level child. </summary>
        /// <param name="entry">Entry to check.</param>
        /// <returns>true if the specified entry is a top-level child; false otherwise.</returns>
        public bool Contains([CanBeNull] Entry entry) {
            return entry != null && _contents.Contains(entry);
        }

        /// <summary> Gathers all entries which belong to this directory, top-level and recursively down. </summary>
        [NotNull]
        public IEnumerable<Entry> FlattenedContents() {
            Contract.Ensures(Contract.Result<IEnumerable<Entry>>() != null);

            foreach (var child in Contents) {
                yield return child;
                var childFolder = (child as FolderEntry);
                if (childFolder == null) continue;

                foreach (var subChild in childFolder.FlattenedContents()) {
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
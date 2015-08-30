using System;
using System.Collections.Generic;

namespace ArchiveCompare {
    /// <summary> Represents an archived directory. </summary>
    public class FolderEntry : Entry {
        /// <summary> Initializes a new instance of the <see cref="FileEntry"/> class. </summary>
        /// <param name="name">Full directory name.</param>
        /// <param name="parent">Parent directory. Null means entry is located at archive's root.</param>
        /// <param name="lastModified">The date when folder was last modified.</param>
        /// <param name="size">Uncompressed size. 0 means uncompressed size is unavailable.</param>
        /// <param name="packedSize">Compressed size. 0 means that either compressed size is unavailable or
        ///  no compression was done on the entry.</param>
        public FolderEntry(string name, FolderEntry parent = null, DateTime? lastModified = null, long size = 0,
            long packedSize = 0) : base(name, parent, lastModified, size, packedSize) {
            Contents = new HashSet<Entry>();
        }

        /// <summary> Gets entries which belong to this directory, top-level only. </summary>
        public HashSet<Entry> Contents { get; private set; }

        /// <summary> Gathers all entries which belong to this directory, top-level and recursively down. </summary>
        public IEnumerable<Entry> FlattenContents() {
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
            return "folder '" + base.ToString() + "'";
        }
    }
}
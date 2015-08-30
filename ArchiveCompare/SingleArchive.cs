using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace ArchiveCompare {
    /// <summary> Archive consisting of single archive file. </summary>
    public class SingleArchive : Archive {
        /// <summary> Initializes a new instance of the <see cref="Archive" /> class. </summary>
        /// <param name="name">Archive name.</param>
        /// <param name="type">Archive type.</param>
        /// <param name="entries">Uninitialized archive entries.</param>
        /// <param name="lastModified">The last modified date for this archive latest modified file.</param>
        /// <param name="physicalSize">Size of the archive as reported by file system.</param>
        /// <param name="size">Uncompressed contents size, 0 if unavailable. </param>
        /// <param name="packedSize">Cmpressed contents size, 0 if unavailable.</param>
        public SingleArchive(string name, ArchiveType type, IEnumerable<Entry> entries, DateTime? lastModified = null,
            long physicalSize = 0, long size = 0, long packedSize = 0)
            : base (name, type, lastModified, physicalSize) {
            Size = size;
            PackedSize = packedSize;
            Initialize(entries);
        }

        /// <summary> Gets or sets the uncompressed contents size, 0 if unavailable. </summary>
        public long Size { get; private set; }

        /// <summary> Gets or sets the compressed contents size, 0 if unavailable. </summary>
        public long PackedSize { get; private set; }

        /// <summary> Gets or sets the number of folders in the archive. </summary>
        public long FolderCount { get; private set; }

        /// <summary> Gets or sets the number of files in the archive. </summary>
        public long FileCount { get; private set; }

        /// <summary> Gets the archive root entries. </summary>
        public IEnumerable<Entry> Contents { get; private set; }

        /// <summary> Converts hierarchical contents into flat sequence. </summary>
        /// <returns> Flattened contents. </returns>
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

        /// <summary> Initializes archive with entries. </summary>
        /// <param name="entries">Uninitialized archive entries.</param>
        private void Initialize(IEnumerable<Entry> entries) {
            Contract.Requires(entries != null);
            if (Contents != null) { throw new InvalidOperationException("Can't initialize archive twice."); }

            var entriesFixed = entries as IList<Entry> ?? entries.ToArray();
            var rootEntries = new List<Entry>();
            // Find all folders in the archive entries:
            var directories = (from entry in entriesFixed
                where entry is FolderEntry
                select entry as FolderEntry).ToDictionary(directory => directory.Name);
            FolderCount = directories.Count;
            // Pass every archive entry and link it to its parent entry.
            long calculatedContentSize = 0;
            long calculatedPackedSize = 0;
            DateTime maxLastModified = DateTime.MinValue;
            foreach (var entry in entriesFixed) {
                if (entry is FileEntry) { FileCount++; }

                // Archive's unpacked and packed sizes are the sum of corresponding entry sizes.
                calculatedContentSize += entry.Size;
                calculatedPackedSize += entry.PackedSize;
                // Archive's last modified is the latest modified file.
                if (entry.LastModifed != null && entry.LastModifed.Value > maxLastModified) {
                    maxLastModified = entry.LastModifed.Value;
                }

                // Look for parent entry to link with:
                var parent = directories.GetValue(Path.GetDirectoryName(entry.Name));
                if (parent != null) {
                    parent.Contents.Add(entry);
                    entry.ParentFolder = parent;
                } else {    // No parent entry means a root entry.
                    rootEntries.Add(entry);
                }
            }

            Contents = rootEntries;
            if (LastModified == null && maxLastModified != DateTime.MinValue) { LastModified = maxLastModified; }
            if (Size == 0) { Size = calculatedContentSize; }
            if (PackedSize == 0) { PackedSize = calculatedPackedSize; }
        }
    }
}
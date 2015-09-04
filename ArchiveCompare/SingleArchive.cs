using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

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
            Contract.Requires(entries != null);
            Contract.Requires(size >= 0);
            Contract.Requires(physicalSize >= 0);
            Contract.Requires(packedSize >= 0);
            Contract.Ensures(Contents != null);

            Size = size;
            PackedSize = packedSize;
            _contents = new List<Entry>();
            Initialize(entries);
        }

        private readonly List<Entry> _contents;

        /// <summary> Gets or sets the uncompressed contents size, 0 if unavailable. </summary>
        public long Size { get; private set; }

        /// <summary> Gets or sets the compressed contents size, 0 if unavailable. </summary>
        public long PackedSize { get; private set; }

        /// <summary> Gets or sets the number of folders in the archive. </summary>
        public long FolderCount { get; private set; }

        /// <summary> Gets or sets the number of files in the archive. </summary>
        public long FileCount { get; private set; }

        /// <summary> Gets the archive root entries. </summary>
        [NotNull]
        public IEnumerable<Entry> Contents => _contents;

        /// <summary> Converts hierarchical contents into flat sequence. </summary>
        /// <returns> Flattened contents. </returns>
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
            string folders = FolderCount > 0 ? FolderCount + " folders" : string.Empty;
            string files = FileCount > 0 ? FileCount + " files" : string.Empty;
            string combined = string.Empty;
            if (files != string.Empty && folders != string.Empty) {
                combined = ": " + files + " & " + folders;
            } else if (files != string.Empty) {
                combined = ": " + files;
            } else if (folders != string.Empty) {
                combined = ": " + folders;
            }

            return base.ToString() + " - (unpacked " + Size + ", packed " + PackedSize + ")" + combined;
        }

        /// <summary> Initializes archive with entries. </summary>
        /// <param name="entries">Uninitialized archive entries.</param>
        private void Initialize(IEnumerable<Entry> entries) {
            Contract.Requires(entries != null);

            var entriesFixed = (entries as IList<Entry>) ?? entries.ToArray();
            Debug.Assert(entriesFixed != null);
            _contents.Clear();
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
                if (entry.LastModified != null && entry.LastModified.Value > maxLastModified) {
                    maxLastModified = entry.LastModified.Value;
                }

                // Look for parent entry to link with:
                var parent = directories.GetValue(Path.GetDirectoryName(entry.Name));
                if (parent != null) {
                    parent.Contents.Add(entry);
                    entry.ParentFolder = parent;
                } else {    // No parent entry means a root entry.
                    _contents.Add(entry);
                }
            }

            if (LastModified == null && maxLastModified != DateTime.MinValue) { LastModified = maxLastModified; }
            if (Size == 0) { Size = calculatedContentSize; }
            if (PackedSize == 0) { PackedSize = calculatedPackedSize; }
        }
    }
}
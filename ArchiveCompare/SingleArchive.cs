using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Archive consisting of single archive file. </summary>
    [DataContract(Name = "singleArchive", IsReference = true, Namespace = "")]
    public class SingleArchive : Archive {
        /// <summary> Initializes a new instance of the <see cref="Archive" /> class. </summary>
        /// <param name="name">Archive name.</param>
        /// <param name="type">Archive type.</param>
        /// <param name="entries">Uninitialized archive entries.</param>
        /// <param name="physicalSize">Size of the archive as reported by file system.</param>
        /// <param name="size">Uncompressed contents size, 0 if unavailable. </param>
        /// <param name="packedSize">Cmpressed contents size, 0 if unavailable.</param>
        /// <param name="lastModified">The last modified date for this archive latest modified file.</param>
        public SingleArchive(string name, ArchiveType type, IEnumerable<Entry> entries,
            long physicalSize = 0, long size = 0, long packedSize = 0, DateTime? lastModified = null)
            : base (name, type, physicalSize, lastModified) {
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

        [DataMember(Name = "contents", IsRequired = true, Order = 10)]
        private readonly List<Entry> _contents;

        /// <summary> Gets or sets the uncompressed contents size, 0 if unavailable. </summary>
        [DataMember(Name = "size", IsRequired = false, Order = 0)]
        public long Size { get; private set; }

        /// <summary> Gets or sets the compressed contents size, 0 if unavailable. </summary>
        [DataMember(Name = "packedSize", IsRequired = false, Order = 1)]
        public long PackedSize { get; private set; }

        /// <summary> Gets or sets the number of files in the archive. </summary>
        [DataMember(Name = "files", IsRequired = true, Order = 2)]
        public long FileCount { get; private set; }

        /// <summary> Gets or sets the number of folders in the archive. </summary>
        [DataMember(Name = "folders", IsRequired = true, Order = 3)]
        public long FolderCount { get; private set; }

        /// <summary> Gets the archive root entries. </summary>
        [NotNull]
        public IEnumerable<Entry> Contents => _contents;

        /// <summary> Converts hierarchical contents into flat sequence. </summary>
        /// <returns> Flattened contents. </returns>
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

        /// <summary> Gets entry represented by the given path. Case does not matter. </summary>
        /// <remarks> For example, take archive containing file entry 'a.txt' in folder 'f',
        /// which is in turn contained in other folder 'rf', which is a root folder.
        /// So path 'rf\f\a.txt' represents 'a.txt' entry, and path 'rf\f' represents 'f' folder entry.
        /// </remarks>
        /// <param name="path">Entry path.</param>
        /// <returns>Found entry or null.</returns>
        [CanBeNull]
        public Entry FindEntry(string path) {
            Contract.Requires(path != null);

            path = Entry.NormalizePath(path);
            var hierarchyLevels = path.Split(Entry.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
            Entry closestFind = null;
            foreach (var hierarchyLevel in hierarchyLevels) {
                if (closestFind == null) {  // We are at root level, so search archive root:
                    closestFind = Contents.FirstOrDefault(entry => entry.IsHomonymousName(hierarchyLevel));
                    continue;
                }
                // We are past root level, so check current folder or file
                // for homonymousity with current hierarchy level:
                var folder = closestFind as FolderEntry;
                if (folder != null) {
                    closestFind = folder.Contents.FirstOrDefault(entry => entry.IsHomonymousName(hierarchyLevel));
                } else {
                    var file = closestFind as FileEntry;
                    if (file != null) {
                        closestFind = file.IsHomonymousName(hierarchyLevel) ? file : null;
                    }
                }
            }

            return (closestFind != null && closestFind.IsHomonymousPath(path)) ? closestFind : null;
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
                select entry as FolderEntry).ToDictionary(directory => directory.Path);

            FolderCount = directories.Count;
            // Pass every archive entry and link it to its parent entry.
            long calculatedContentSize = 0;
            long calculatedPackedSize = 0;
            DateTime maxLastModified = DateTime.MinValue;
            foreach (var entry in entriesFixed) {
                if (entry is FileEntry) {
                    FileCount++;
                }

                // Archive's unpacked and packed sizes are the sum of corresponding entry sizes.
                calculatedContentSize += entry.Size;
                calculatedPackedSize += entry.PackedSize;
                // Archive's last modified is the latest modified file.
                if (entry.LastModified != null && entry.LastModified.Value > maxLastModified) {
                    maxLastModified = entry.LastModified.Value;
                }

                // Look for parent entry to link with:
                string parentName = Path.GetDirectoryName(entry.Path);
                var parent = directories.GetValue(parentName);
                if (parent != null) {
                    parent.Add(entry);
                    entry.ParentFolder = parent;
                } else {    // No parent entry means a root entry.
                    _contents.Add(entry);
                }
            }

            if (LastModified == null && maxLastModified != DateTime.MinValue) { LastModified = maxLastModified; }
            if (Size == 0) { Size = calculatedContentSize; }
            if (PackedSize == 0) { PackedSize = calculatedPackedSize; }
        }

        [ContractInvariantMethod]
        private void ObjectInvariant() {
            Contract.Invariant(Contents != null);
            Contract.Invariant(Size >= 0);
            Contract.Invariant(PackedSize >= 0);
            Contract.Invariant(FileCount >= 0);
            Contract.Invariant(FolderCount >= 0);
        }
    }
}
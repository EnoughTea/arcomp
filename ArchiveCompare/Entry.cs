using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents single archive entry. </summary>
    public abstract class Entry {
        #region Static methods

        /// <summary> Finds differences between archives. </summary>
        /// <param name="left">'Left' archive.</param>
        /// <param name="right">'Right' archive.</param>
        public static IEnumerable<EntryTraitDifference> Diff(Entry left, Entry right) {
            Contract.Requires(left != null);
            Contract.Requires(right != null);

            return Comparisons
                .Select(cmpType => (EntryTraitDifference)Activator.CreateInstance(cmpType, left, right))
                .Where(cmp => cmp != null && cmp.ComparisonExists && cmp.DifferenceExists);
        }

        #endregion

        /// <summary> Initializes a new instance of the <see cref="Entry" /> class. </summary>
        /// <param name="name">Full file or folder name.</param>
        /// <param name="lastModified">The date when file was last modified.</param>
        /// <param name="size">Uncompressed size. 0 means uncompressed size is unavailable.</param>
        /// <param name="packedSize">Compressed size. 0 means that either compressed size is unavailable or
        ///     no compression was done on the entry.</param>
        /// <param name="parent">Parent directory. Null means entry is located at archive's root.</param>
        protected Entry(string name, DateTime? lastModified = null, long size = 0, long packedSize = 0,
            FolderEntry parent = null) {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(size >= 0);
            Contract.Requires(packedSize >= 0);

            Name = name;
            ParentFolder = parent;
            Size = size;
            PackedSize = packedSize;
            LastModified = lastModified;
        }

        /// <summary> Gets the archive entry name.</summary>
        [NotNull]
        public string Name { get; }

        /// <summary> Gets or sets the date when file was last modified.
        ///  Null means modified date in unavailable.</summary>
        public DateTime? LastModified { get; }

        /// <summary> Gets or sets the uncompressed size. 0 means that entry is a folder or
        ///  uncompressed size is unavailable. </summary>
        public long Size { get; }

        /// <summary> Gets or sets the compressed size.
        /// 0 means that either entry is a folder, or no compression was done on the entry,
        /// or compressed size is unavailable.
        /// </summary>
        public long PackedSize { get; }

        /// <summary> Gets or sets the parent directory. Null means root. </summary>
        public FolderEntry ParentFolder { get; internal set; }

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString() {
            string lastModified = (LastModified != null) ? ", modified on " + LastModified : string.Empty;
            return Name + lastModified;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant() {
            Contract.Invariant(string.IsNullOrWhiteSpace(Name));
            Contract.Invariant(Size >= 0);
            Contract.Invariant(PackedSize >= 0);
        }

        private static readonly HashSet<Type> Comparisons = new HashSet<Type> {
            typeof(EntryFileNameDifference), typeof(EntryParentFolderDifference), typeof(EntryLastModifiedDifference),
            typeof(EntrySizeDifference), typeof(EntryPackedSizeDifference), typeof(EntryCrcDifference)
        };
    }
}

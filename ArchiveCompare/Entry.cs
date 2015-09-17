using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents single archive entry. </summary>
    public abstract class Entry {
        /// <summary> Character separating hierarchy levels in entry path. </summary>
        public static readonly char[] PathSeparator = { '\\' };

        #region Static methods

        /// <summary> Determines whether two entries (possibly from different archives) have the homonymous parent
        ///  hierarchy all the way up to root. </summary>
        /// <remarks>
        ///  Entries with same path in different archives are considired to have the same parent hierarchy.
        /// </remarks>
        /// <returns>
        ///  true if both entries has the same-named parent hierarchy all the way up to root; false otherwise.
        /// </returns>
        public static bool IsHomonymousParentPath([CanBeNull] Entry first, [CanBeNull] Entry second) {
            if (first == null || second == null) { return false; }
            // If at least one entry is a root entry, 'true' is only possible when other entry is a root entry too:
            if (first.ParentFolder == null || second.ParentFolder == null) {
                return first.ParentFolder == second.ParentFolder;
            }

            return first.ParentFolder.IsHomonymousPath(second.ParentFolder);
        }

        /// <summary> Finds differences between archives. </summary>
        /// <param name="left">'Left' archive.</param>
        /// <param name="right">'Right' archive.</param>
        public static IEnumerable<EntryTraitDifference> PropertiesDiff(Entry left, Entry right) {
            Contract.Requires(left != null);
            Contract.Requires(right != null);

            return Comparisons
                .Select(cmpType => (EntryTraitDifference)Activator.CreateInstance(cmpType, left, right))
                .Where(cmp => cmp != null && cmp.ComparisonExists && cmp.DifferenceExists);
        }

        #endregion

        /// <summary> Initializes a new instance of the <see cref="Entry" /> class. </summary>
        /// <param name="path">Full file or folder name.</param>
        /// <param name="lastModified">The date when file was last modified.</param>
        /// <param name="size">Uncompressed size. 0 means uncompressed size is unavailable.</param>
        /// <param name="packedSize">Compressed size. 0 means that either compressed size is unavailable or
        ///     no compression was done on the entry.</param>
        /// <param name="parent">Parent directory. Null means entry is located at archive's root.</param>
        protected Entry(string path, DateTime? lastModified = null, long size = 0, long packedSize = 0,
            FolderEntry parent = null) {
            Contract.Requires(!string.IsNullOrWhiteSpace(path));
            Contract.Requires(size >= 0);
            Contract.Requires(packedSize >= 0);

            Path = NormalizePath(path);
            ParentFolder = parent;
            Size = size;
            PackedSize = packedSize;
            LastModified = lastModified;
        }

        /// <summary> Gets the archive entry full name, with its parent folders up to root.</summary>
        [NotNull]
        public string Path { get; }

        /// <summary> Gets the name of this entry. </summary>
        public abstract string Name { get; }

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

        /// <summary> Determines whether this entry has the same name as the specified entry. Ignores case. </summary>
        /// <param name="entry">The entry which name to check for homonymousity.</param>
        /// <returns>true if entries have the same name; false otherwise.</returns>
        public bool IsHomonymousName([CanBeNull] Entry entry) {
            return (entry != null) && IsHomonymousName(entry.Name);
        }

        /// <summary> Determines whether this entry has the same name as the specified entry. Ignores case. </summary>
        /// <param name="entryName">The entry name to check for homonymousity.</param>
        /// <returns>true if entries have the same name; false otherwise.</returns>
        public bool IsHomonymousName([CanBeNull] string entryName) {
            return string.Equals(Name, entryName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary> Determines whether this entry has the same path as the specified entry. Ignores case.</summary>
        /// <remarks> Entries for different archives with same path will return true. </remarks>
        /// <param name="entry">The entry which path to check for homonymousity.</param>
        /// <returns>true if entries have the same path; false otherwise.</returns>
        public bool IsHomonymousPath([CanBeNull] Entry entry) {
            return (entry != null) && IsHomonymousPath(entry.Path);
        }

        /// <summary> Determines whether this entry path is the same with the specified path. Ignores case.</summary>
        /// <remarks> Entries for different archives with same path will return true. </remarks>
        /// <param name="entryPath">The entry path to check for homonymousity.</param>
        /// <returns>true if entries have the same path; false otherwise.</returns>
        public bool IsHomonymousPath([CanBeNull] string entryPath) {
            return string.Equals(Path, NormalizePath(entryPath), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString() {
            string lastModified = (LastModified != null) ? ", modified on " + LastModified : string.Empty;
            return Path + lastModified;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant() {
            Contract.Invariant(!string.IsNullOrWhiteSpace(Path));
            Contract.Invariant(Size >= 0);
            Contract.Invariant(PackedSize >= 0);
        }

        [CanBeNull, ContractAnnotation("path: null => null;path: notnull => notnull")]
        internal static string NormalizePath([CanBeNull] string path) {
            return path?.Trim().Replace('/', PathSeparator[0]).TrimEnd(PathSeparator);
        }

        private static readonly HashSet<Type> Comparisons = new HashSet<Type> {
            typeof(EntryTypeDifference), typeof(EntryFileNameDifference), typeof(EntryParentFolderDifference),
            typeof(EntryLastModifiedDifference), typeof(EntrySizeDifference), typeof(EntryPackedSizeDifference),
            typeof(EntryCrcDifference), typeof(EntryFileCountDifference), typeof(EntryFolderCountDifference)
        };
    }
}

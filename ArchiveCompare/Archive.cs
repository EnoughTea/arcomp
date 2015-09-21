using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Base class for all archives. </summary>
    [DataContract(Name = "archive", IsReference = true, Namespace = "")]
    public abstract class Archive {
        #region Static methods

        /// <summary> Finds property differences between archives. </summary>
        /// <param name="left">'Left' archive.</param>
        /// <param name="right">'Right' archive.</param>
        /// <returns>Found property differences.</returns>
        public static IEnumerable<ArchiveTraitDifference> PropertiesDiff(Archive left, Archive right) {
            Contract.Requires(left != null);
            Contract.Requires(right != null);

            return Comparisons
                .Select(cmpType => (ArchiveTraitDifference)Activator.CreateInstance(cmpType, left, right))
                .Where(cmp => cmp != null && cmp.ComparisonExists && cmp.DifferenceExists);
        }

        /// <summary> Finds entry differences between archives. </summary>
        /// <param name="left">'Left' archive.</param>
        /// <param name="right">'Right' archive.</param>
        /// <returns> Found entry differences. </returns>
        public static IEnumerable<EntryVersionsComparison> EntriesDiff(Archive left, Archive right) {
            Contract.Requires(left != null);
            Contract.Requires(right != null);

            var actualLeft = (left is SingleArchive) ? (SingleArchive)left : ((SplitArchive)left).Nested;
            var actualRight = (right is SingleArchive) ? (SingleArchive)right : ((SplitArchive)right).Nested;
            var leftEntries = actualLeft.FlattenedContents().ToList();
            var rightEntries = new HashSet<Entry>(actualRight.FlattenedContents());

            for (int i = leftEntries.Count - 1; i >= 0; i--) {
                var leftEntry = leftEntries[i];
                leftEntries.RemoveAt(i);
                var correspondingRight = rightEntries.FirstOrDefault(f => f.IsHomonymousPath(leftEntry));
                if (correspondingRight != null) {
                    rightEntries.Remove(correspondingRight);
                }

                var cmp = new EntryVersionsComparison(leftEntry, correspondingRight);
                if (cmp.State != EntryModificationState.Same) {
                    yield return cmp;
                }
            }

            foreach (var rightEntry in rightEntries) {
                yield return new EntryVersionsComparison(null, rightEntry);
            }
        }

        /// <summary> Converts archive type to string representation. </summary>
        /// <param name="type">Archive type.</param>
        /// <returns>String representation of the given archive type.</returns>
        public static string TypeToString(ArchiveType type) {
            Contract.Ensures(Contract.Result<string>() != null);

            return TypesToNames.GetValue(type)?.ToLowerInvariant() ?? string.Empty;
        }

        /// <summary> Converts archive type string representation to the archive type. </summary>
        /// <param name="typeName">Archive type string representation.</param>
        /// <returns>Archive type corresponding to the given string.</returns>
        public static ArchiveType StringToType([CanBeNull] string typeName) {
            return !string.IsNullOrEmpty(typeName)
                ? NamesToTypes.GetValue(typeName.ToLowerInvariant())
                : ArchiveType.Unknown;
        }

        #endregion

        /// <summary> Initializes a new instance of the <see cref="Archive" /> class. </summary>
        /// <param name="path">Archive file path.</param>
        /// <param name="type">Archive type.</param>
        /// <param name="physicalSize">Size of the archive as reported by file system.</param>
        /// <param name="lastModified">The last modified date for this archive latest modified file.</param>
        protected Archive(string path, ArchiveType type, long physicalSize = 0, DateTime? lastModified = null) {
            Contract.Requires(!string.IsNullOrWhiteSpace(path));
            Contract.Requires(physicalSize >= 0);

            Path = path;
            Type = type;
            LastModified = lastModified;
            PhysicalSize = physicalSize;
        }

        private DateTime? _lastModified;

        /// <summary> Gets the archive file path.</summary>
        [NotNull, DataMember(Name = "path", IsRequired = true, Order = 0)]
        public string Path { get; }

        /// <summary> Gets this archive name with extension. </summary>
        public string Name => System.IO.Path.GetFileName(Path);

        /// <summary> Gets the archive type. </summary>
        [DataMember(Name = "type", Order = 1)]
        public ArchiveType Type { get; }

        /// <summary> Gets the last modified date of the latest modified file in the archive.
        ///  Null means latest modified file date in unavailable.</summary>
        [DataMember(Name = "modified", IsRequired = false, EmitDefaultValue = false, Order = 2)]
        public DateTime? LastModified {
            get {
                var split = this as SplitArchive;
                return (split != null && _lastModified == null) ? split.Nested.LastModified : _lastModified;
            }

            internal set { _lastModified = value; }
        }

        /// <summary> Gets the size of the archive as reported by file system. For split archives means
        ///  user-defined part size (last part could be of any size). </summary>
        [DataMember(Name = "physSize", Order = 3)]
        public long PhysicalSize { get; }

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString() {
            string lastModified = (LastModified != null) ? ", modified on " + LastModified : string.Empty;
            return Path + lastModified + ", physical size " + PhysicalSize;
        }

        /// <summary> Gets the archive root entries. </summary>
        public abstract IEnumerable<Entry> Contents { get; }

        [ContractInvariantMethod]
        private void ObjectInvariant() {
            Contract.Invariant(!string.IsNullOrWhiteSpace(Path));
            Contract.Invariant(PhysicalSize >= 0);
        }

        private static readonly Dictionary<ArchiveType, string> TypesToNames = new Dictionary<ArchiveType, string> {
            { ArchiveType.BZip2, "bzip2" },
            { ArchiveType.GZip, "gzip" },
            { ArchiveType.Mbr, "mbr" },
            { ArchiveType.Pe, "pe" },
            { ArchiveType.Rar, "rar" },
            { ArchiveType.Tar, "tar" },
            { ArchiveType.Vhd, "vhd" },
            { ArchiveType.Xz, "xz" },
            { ArchiveType.Zip, "zip" },
            { ArchiveType.SevenZip, "7z" },
            { ArchiveType.Split, "split" },
            { ArchiveType.Unknown, "unknown" }
        };

        private static readonly Dictionary<string, ArchiveType> NamesToTypes = TypesToNames
            .ToDictionary(key => key.Value, value => value.Key);


        private static readonly HashSet<Type> Comparisons = new HashSet<Type> {
            typeof(ArchiveTypeDifference), typeof(ArchiveFileNameDifference), typeof(ArchiveFileCountDifference),
            typeof(ArchiveFolderCountDifference), typeof(ArchiveLastModifiedDifference),
            typeof(ArchivePackedSizeDifference), typeof(ArchivePhysicalSizeDifference), typeof(ArchiveSizeDifference),
            typeof(ArchiveTotalPhysicalSizeDifference)
        };
    }
}
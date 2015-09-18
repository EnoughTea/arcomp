using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an archive difference by folder count. </summary>
    [DataContract(Name = "archiveFolderCountDiff", IsReference = true, Namespace = "")]
    public class ArchiveFolderCountDifference : ArchiveTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="ArchiveFolderCountDifference"/> class. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        public ArchiveFolderCountDifference([CanBeNull] Archive left, [CanBeNull] Archive right)
            : base(left, right) {
        }

        /// <summary> Gets the left folder count. </summary>
        [DataMember(Name = "lFolders", Order = 0)]
        public long LeftFolderCount { get; private set; }

        /// <summary> Gets the right folder count. </summary>
        [DataMember(Name = "rFolders", Order =1)]
        public long RightFolderCount { get; private set; }

        /// <summary> Gets a value indicating whether the archives differ by this trait. </summary>
        public override bool DifferenceExists => LeftFolderCount != RightFolderCount;

        /// <summary> Returns a <see cref="System.String" /> that represents this instance. </summary>
        /// <returns> A <see cref="System.String" /> that represents this instance. </returns>
        public override string ToString() {
            return base.ToString() + $" ({LeftFolderCount} v {RightFolderCount})";
        }

        /// <summary> Initializes comparison from two single archives. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        /// <returns>true if comparison of two single archives by this trait can be performed;
        ///  false otherwise.</returns>
        protected override bool InitFromSingles(SingleArchive left, SingleArchive right) {
            LeftFolderCount = left.FolderCount;
            RightFolderCount = right.FolderCount;
            return true;
        }
    }
}
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an archive difference by file count. </summary>
    [DataContract(Name = "archiveFileCountDiff", IsReference = true, Namespace = "")]
    public class ArchiveFileCountDifference : ArchiveTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="ArchiveFileCountDifference"/> class. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        public ArchiveFileCountDifference([CanBeNull] Archive left, [CanBeNull] Archive right)
            : base(left, right) {
        }

        /// <summary> Gets the left file count. </summary>
        [DataMember(Name = "lFiles", Order = 0)]
        public long LeftFileCount { get; private set; }

        /// <summary> Gets the right file count. </summary>
        [DataMember(Name = "rFiles", Order = 1)]
        public long RightFileCount { get; private set; }

        /// <summary> Gets a value indicating whether the archives differ by this trait. </summary>
        public override bool DifferenceExists => LeftFileCount != RightFileCount;

        /// <summary> Returns a <see cref="System.String" /> that represents this instance. </summary>
        /// <returns> A <see cref="System.String" /> that represents this instance. </returns>
        public override string ToString() {
            return base.ToString() + $" ({LeftFileCount} v {RightFileCount})";
        }

        /// <summary> Initializes comparison from two single archives. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        /// <returns>true if comparison of two single archives by this trait can be performed;
        ///  false otherwise.</returns>
        protected override bool InitFromSingles(SingleArchive left, SingleArchive right) {
            LeftFileCount = left.FileCount;
            RightFileCount = right.FileCount;
            return true;
        }
    }
}
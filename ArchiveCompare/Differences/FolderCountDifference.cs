using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an archive difference by file count. </summary>
    public class FolderCountDifference : ArchiveTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="FileCountDifference"/> class. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        public FolderCountDifference([CanBeNull] Archive left, [CanBeNull] Archive right)
            : base(left, right) {
        }

        /// <summary> Gets the left folder count. </summary>
        public long LeftFolderCount { get; private set; }

        /// <summary> Gets the right folder count. </summary>
        public long RightFolderCount { get; private set; }

        /// <summary> Gets a value indicating whether the archives differ by this trait. </summary>
        public override bool DifferenceExists => LeftFolderCount != RightFolderCount;

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
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an archive difference by file count. </summary>
    public class FileCountDifference : ArchiveTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="FileCountDifference"/> class. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        public FileCountDifference([CanBeNull] Archive left, [CanBeNull] Archive right)
            : base(left, right) {
        }

        /// <summary> Gets the left file count. </summary>
        public long LeftFileCount { get; private set; }

        /// <summary> Gets the right file count. </summary>
        public long RightFileCount { get; private set; }

        /// <summary> Gets a value indicating whether the archives differ by this trait. </summary>
        public override bool DifferenceExists => LeftFileCount != RightFileCount;

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
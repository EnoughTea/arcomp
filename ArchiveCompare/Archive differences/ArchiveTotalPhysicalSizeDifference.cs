using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an archive difference by total physical size. </summary>
    public class ArchiveTotalPhysicalSizeDifference : ArchiveTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="ArchiveTotalPhysicalSizeDifference"/> class. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        public ArchiveTotalPhysicalSizeDifference([CanBeNull] Archive left, [CanBeNull] Archive right)
            : base(left, right) {
        }

        /// <summary> Gets the left total physical size. </summary>
        public long LeftTotalPhysicalSize { get; private set; }

        /// <summary> Gets the right total physical size. </summary>
        public long RightTotalPhysicalSize { get; private set; }

        /// <summary> Gets a value indicating whether the archives differ by this trait. </summary>
        public override bool DifferenceExists => LeftTotalPhysicalSize != RightTotalPhysicalSize;

        /// <summary> Returns a <see cref="System.String" /> that represents this instance. </summary>
        /// <returns> A <see cref="System.String" /> that represents this instance. </returns>
        public override string ToString() {
            return base.ToString() + $" ({LeftTotalPhysicalSize} v {RightTotalPhysicalSize})";
        }

        /// <summary> Initializes comparison from single and split archives. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        /// <returns>true if comparison of single and split archives by this trait can be performed;
        ///  false otherwise.</returns>
        protected override bool InitFromSingleAndSplit(SingleArchive left, SplitArchive right) {
            LeftTotalPhysicalSize = 0;
            RightTotalPhysicalSize = right.TotalPhysicalSize;
            return true;
        }

        /// <summary> Initializes comparison from split and single archives. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        /// <returns>true if comparison of split and single archives by this trait can be performed;
        ///  false otherwise.</returns>
        protected override bool InitFromSplitAndSingle(SplitArchive left, SingleArchive right) {
            LeftTotalPhysicalSize = left.TotalPhysicalSize;
            RightTotalPhysicalSize = 0;
            return true;
        }

        /// <summary> Initializes comparison from two split archives. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        /// <returns>true if comparison of two split archives by this trait can be performed;
        ///  false otherwise.</returns>
        protected override bool InitFromSplits(SplitArchive left, SplitArchive right) {
            LeftTotalPhysicalSize = left.TotalPhysicalSize;
            RightTotalPhysicalSize = right.TotalPhysicalSize;
            return true;
        }
    }
}

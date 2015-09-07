using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an archive difference by packed size. </summary>
    public class ArchivePackedSizeDifference : ArchiveTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="ArchivePackedSizeDifference"/> class. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        public ArchivePackedSizeDifference([CanBeNull] Archive left, [CanBeNull] Archive right)
            : base(left, right) {
        }

        /// <summary> Gets the left packed size. </summary>
        public long LeftPackedSize { get; private set; }

        /// <summary> Gets the right packed size. </summary>
        public long RightPackedSize { get; private set; }

        /// <summary> Gets a value indicating whether the archives differ by this trait. </summary>
        public override bool DifferenceExists => LeftPackedSize != RightPackedSize;

        /// <summary> Initializes comparison from two single archives. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        /// <returns>true if comparison of two single archives by this trait can be performed;
        ///  false otherwise.</returns>
        protected override bool InitFromSingles(SingleArchive left, SingleArchive right) {
            LeftPackedSize = left.PackedSize;
            RightPackedSize = right.PackedSize;
            return true;
        }
    }
}
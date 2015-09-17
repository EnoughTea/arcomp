using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an archive difference by physical size. </summary>
    public class ArchivePhysicalSizeDifference : ArchiveTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="ArchivePhysicalSizeDifference"/> class. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        public ArchivePhysicalSizeDifference([CanBeNull] Archive left, [CanBeNull] Archive right)
            : base(left, right) {
        }

        /// <summary> Gets the left physical size. </summary>
        public long LeftPhysicalSize { get; private set; }

        /// <summary> Gets the right physical size. </summary>
        public long RightPhysicalSize { get; private set; }

        /// <summary> Gets a value indicating whether the archives differ by this trait. </summary>
        public override bool DifferenceExists => LeftPhysicalSize != RightPhysicalSize;

        /// <summary> Returns a <see cref="System.String" /> that represents this instance. </summary>
        /// <returns> A <see cref="System.String" /> that represents this instance. </returns>
        public override string ToString() {
            return base.ToString() + $" ({LeftPhysicalSize} v {RightPhysicalSize})";
        }

        /// <summary> Initializes comparison from any two archives. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        /// <returns>true if comparison of any two archives by this trait can be performed; false otherwise.</returns>
        protected override bool InitFromArchives(Archive left, Archive right) {
            LeftPhysicalSize = left.PhysicalSize;
            RightPhysicalSize = right.PhysicalSize;
            return true;
        }
    }
}
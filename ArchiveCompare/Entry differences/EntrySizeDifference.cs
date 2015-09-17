using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an entry difference by unpacked size. </summary>
    public class EntrySizeDifference : EntryTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="EntrySizeDifference" /> class. </summary>
        /// <param name="left">Left entry.</param>
        /// <param name="right">Right entry.</param>
        public EntrySizeDifference([CanBeNull] Entry left, [CanBeNull] Entry right)
            : base(left, right) {
        }

        /// <summary> Gets the left unpacked size. </summary>
        public long LeftSize { get; private set; }

        /// <summary> Gets the right unpacked size. </summary>
        public long RightSize { get; private set; }

        /// <summary> Gets a value indicating whether the entries differ by this trait. </summary>
        public override bool DifferenceExists => LeftSize != RightSize;

        /// <summary> Returns a <see cref="System.String" /> that represents this instance. </summary>
        /// <returns> A <see cref="System.String" /> that represents this instance. </returns>
        public override string ToString() {
            return base.ToString() + $" ({LeftSize} v {RightSize})";
        }

        /// <summary> Initializes comparison from any two entries. </summary>
        /// <param name="left">Left entry.</param>
        /// <param name="right">Right entry.</param>
        /// <returns>true if comparison of two entries by this trait can be performed; false otherwise.</returns>
        protected override bool InitFromEntries(Entry left, Entry right) {
            LeftSize = left.Size;
            RightSize = right.Size;
            return true;
        }
    }
}
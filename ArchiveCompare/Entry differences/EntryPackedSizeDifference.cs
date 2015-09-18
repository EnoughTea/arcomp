using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an entry difference by packed size. </summary>
    [DataContract(Name = "ePackedSizeDiff", IsReference = true, Namespace = "")]
    public class EntryPackedSizeDifference : EntryTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="EntryPackedSizeDifference" /> class. </summary>
        /// <param name="left">Left entry.</param>
        /// <param name="right">Right entry.</param>
        public EntryPackedSizeDifference([CanBeNull] Entry left, [CanBeNull] Entry right)
            : base(left, right) {
        }

        /// <summary> Gets the left packed size. </summary>
        [DataMember(Name = "lPackedSize", Order = 0)]
        public long LeftPackedSize { get; private set; }

        /// <summary> Gets the right packed size. </summary>
        [DataMember(Name = "rPackedSize", Order = 1)]
        public long RightPackedSize { get; private set; }

        /// <summary> Gets a value indicating whether the entries differ by this trait. </summary>
        public override bool DifferenceExists => LeftPackedSize != RightPackedSize;

        /// <summary> Returns a <see cref="System.String" /> that represents this instance. </summary>
        /// <returns> A <see cref="System.String" /> that represents this instance. </returns>
        public override string ToString() {
            return base.ToString() + $" ({LeftPackedSize} v {RightPackedSize})";
        }

        /// <summary> Initializes comparison from any two entries. </summary>
        /// <param name="left">Left entry.</param>
        /// <param name="right">Right entry.</param>
        /// <returns>true if comparison of two entries by this trait can be performed; false otherwise.</returns>
        protected override bool InitFromEntries(Entry left, Entry right) {
            LeftPackedSize = left.PackedSize;
            RightPackedSize = right.PackedSize;
            return true;
        }
    }
}
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an entry difference by type. </summary>
    [DataContract(Name = "eTypeDiff", IsReference = true, Namespace = "")]
    public class EntryTypeDifference : EntryTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="EntryTypeDifference" /> class. </summary>
        /// <param name="left">Left entry.</param>
        /// <param name="right">Right entry.</param>
        public EntryTypeDifference([CanBeNull] Entry left, [CanBeNull] Entry right)
            : base(left, right) {
        }

        /// <summary> Gets the left entry type. </summary>
        [DataMember(Name = "lType", Order = 0)]
        public EntryType LeftEntryType { get; private set; }

        /// <summary> Gets the right entry type. </summary>
        [DataMember(Name = "rType", Order = 1)]
        public EntryType RightEntryType { get; private set; }

        /// <summary> Gets a value indicating whether the entries differ by this trait. </summary>
        public override bool DifferenceExists => LeftEntryType != RightEntryType;

        /// <summary> Returns a <see cref="System.String" /> that represents this instance. </summary>
        /// <returns> A <see cref="System.String" /> that represents this instance. </returns>
        public override string ToString() {
            return base.ToString() + $" ({LeftEntryType} v {RightEntryType})";
        }

        /// <summary> Initializes comparison from any two entries. </summary>
        /// <param name="left">Left entry.</param>
        /// <param name="right">Right entry.</param>
        /// <returns>true if comparison of two entries by this trait can be performed; false otherwise.</returns>
        protected override bool InitFromEntries(Entry left, Entry right) {
            LeftEntryType = Entry.Type(left);
            RightEntryType = Entry.Type(right);
            return true;
        }
    }
}
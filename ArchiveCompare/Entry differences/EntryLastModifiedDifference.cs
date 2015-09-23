using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an entry difference by last modified date. </summary>
    [DataContract(Name = "eModifiedDiff", IsReference = true, Namespace = "")]
    public class EntryLastModifiedDifference : EntryTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="EntryLastModifiedDifference" /> class. </summary>
        /// <param name="left">Left entry.</param>
        /// <param name="right">Right entry.</param>
        public EntryLastModifiedDifference([CanBeNull] Entry left, [CanBeNull] Entry right)
            : base(left, right) {
        }

        /// <summary> Gets the left last modified date. </summary>
        [DataMember(Name = "lModified", Order = 0)]
        public DateTime? LeftLastModified { get; private set; }

        /// <summary> Gets the right last modified date. </summary>
        [DataMember(Name = "rModified", Order = 1)]
        public DateTime? RightLastModified { get; private set; }

        /// <summary> Gets a value indicating whether the entries differ by this trait. </summary>
        public override bool DifferenceExists =>
                    LeftLastModified?.ToUniversalTime() != RightLastModified?.ToUniversalTime();

        /// <summary> Returns a <see cref="System.String" /> that represents this instance. </summary>
        /// <returns> A <see cref="System.String" /> that represents this instance. </returns>
        public override string ToString() {
            return base.ToString() + $" ({LeftLastModified} v {RightLastModified})";
        }

        /// <summary> Initializes comparison from any two entries. </summary>
        /// <param name="left">Left entry.</param>
        /// <param name="right">Right entry.</param>
        /// <returns>true if comparison of two entries by this trait can be performed; false otherwise.</returns>
        protected override bool InitFromEntries(Entry left, Entry right) {
            LeftLastModified = left.LastModified;
            RightLastModified = right.LastModified;
            return true;
        }
    }
}
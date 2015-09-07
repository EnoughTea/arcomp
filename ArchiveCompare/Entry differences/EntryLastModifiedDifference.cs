using System;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an entry difference by last modified date. </summary>
    public class EntryLastModifiedDifference : EntryTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="EntryLastModifiedDifference" /> class. </summary>
        /// <param name="left">Left entry.</param>
        /// <param name="right">Right entry.</param>
        public EntryLastModifiedDifference([CanBeNull] Entry left, [CanBeNull] Entry right)
            : base(left, right) {
        }

        /// <summary> Gets the left last modified date. </summary>
        public DateTime? LeftLastModified { get; private set; }

        /// <summary> Gets the right last modified date. </summary>
        public DateTime? RightLastModified { get; private set; }

        /// <summary> Gets a value indicating whether the entries differ by this trait. </summary>
        public override bool DifferenceExists => LeftLastModified != RightLastModified;

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
using System;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an archive difference by last modified date. </summary>
    public class ArchiveLastModifiedDifference : ArchiveTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="ArchiveLastModifiedDifference"/> class. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        public ArchiveLastModifiedDifference([CanBeNull] Archive left, [CanBeNull] Archive right)
            : base(left, right) {
        }

        /// <summary> Gets the left last modified date. </summary>
        public DateTime? LeftLastModified { get; private set; }

        /// <summary> Gets the right last modified date. </summary>
        public DateTime? RightLastModified { get; private set; }

        /// <summary> Gets a value indicating whether the archives differ by this trait. </summary>
        public override bool DifferenceExists => LeftLastModified != RightLastModified;

        /// <summary> Returns a <see cref="System.String" /> that represents this instance. </summary>
        /// <returns> A <see cref="System.String" /> that represents this instance. </returns>
        public override string ToString() {
            return base.ToString() + $" ({LeftLastModified} v {RightLastModified})";
        }

        /// <summary> Initializes comparison from any two archives. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        /// <returns>true if comparison of any two archives by this trait can be performed; false otherwise.</returns>
        protected override bool InitFromArchives(Archive left, Archive right) {
            LeftLastModified = left.LastModified;
            RightLastModified = right.LastModified;
            return true;
        }
    }
}
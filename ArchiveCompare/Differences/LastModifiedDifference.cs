using System;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an archive difference by file count. </summary>
    public class LastModifiedDifference : ArchiveTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="FileCountDifference"/> class. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        public LastModifiedDifference([CanBeNull] Archive left, [CanBeNull] Archive right)
            : base(left, right) {
        }

        /// <summary> Gets the left last modified date. </summary>
        public DateTime? LeftLastModified { get; private set; }

        /// <summary> Gets the right last modified date. </summary>
        public DateTime? RightLastModified { get; private set; }

        /// <summary> Gets a value indicating whether the archives differ by this trait. </summary>
        public override bool DifferenceExists => LeftLastModified != RightLastModified;

        /// <summary> Initializes comparison from two single archives. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        /// <returns>true if comparison of two single archives by this trait can be performed;
        ///  false otherwise.</returns>
        protected override bool InitFromSingles(SingleArchive left, SingleArchive right) {
            LeftLastModified = left.LastModified;
            RightLastModified = right.LastModified;
            return true;
        }
    }
}
using System;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an archive difference by file name. </summary>
    public class FileNameDifference : ArchiveTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="FileCountDifference"/> class. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        public FileNameDifference([CanBeNull] Archive left, [CanBeNull] Archive right)
            : base(left, right) {
        }

        /// <summary> Gets the left file name. </summary>
        public string LeftFileName { get; private set; }

        /// <summary> Gets the right file name. </summary>
        public string RightFileName { get; private set; }

        /// <summary> Gets a value indicating whether the archives differ by this trait. </summary>
        public override bool DifferenceExists => !string.Equals(LeftFileName, RightFileName, StringComparison.Ordinal);

        /// <summary> Initializes comparison from two single archives. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        /// <returns>true if comparison of two single archives by this trait can be performed;
        ///  false otherwise.</returns>
        protected override bool InitFromSingles(SingleArchive left, SingleArchive right) {
            LeftFileName = left.Name;
            RightFileName = right.Name;
            return true;
        }
    }
}
using System;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an archive difference by file name. </summary>
    public class ArchiveFileNameDifference : ArchiveTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="ArchiveFileNameDifference" /> class. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        /// <param name="caseSensitive">if set to <c>true</c>, filename comparison will be case sensitive.</param>
        public ArchiveFileNameDifference([CanBeNull] Archive left, [CanBeNull] Archive right,
            bool caseSensitive = false)
            : base(left, right) {
            CaseSensitive = caseSensitive;
        }

        /// <summary> Gets a value indicating whether filename comparison is case sensitive. </summary>
        public bool CaseSensitive { get; }

        /// <summary> Gets the left file name. </summary>
        public string LeftFileName { get; private set; }

        /// <summary> Gets the right file name. </summary>
        public string RightFileName { get; private set; }

        /// <summary> Gets a value indicating whether the archives differ by this trait. </summary>
        public override bool DifferenceExists => !string.Equals(LeftFileName, RightFileName,
            CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);

        /// <summary> Initializes comparison from any two archives. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        /// <returns>true if comparison of any two archives by this trait can be performed; false otherwise.</returns>
        protected override bool InitFromArchives(Archive left, Archive right) {
            LeftFileName = left.Name;
            RightFileName = right.Name;
            return true;
        }
    }
}
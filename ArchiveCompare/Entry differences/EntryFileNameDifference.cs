using System;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an entry difference by file name. </summary>
    public class EntryFileNameDifference : EntryTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="ArchiveFileNameDifference" /> class. </summary>
        /// <param name="left">Left entry.</param>
        /// <param name="right">Right entry.</param>
        /// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
        public EntryFileNameDifference([CanBeNull] Entry left, [CanBeNull] Entry right, bool caseSensitive = false)
            : base(left, right) {
            CaseSensitive = caseSensitive;
        }

        /// <summary> Gets a value indicating whether filename comparison is case sensitive. </summary>
        public bool CaseSensitive { get; }

        /// <summary> Gets the left file name. </summary>
        public string LeftFileName { get; private set; }

        /// <summary> Gets the right file name. </summary>
        public string RightFileName { get; private set; }

        /// <summary> Gets a value indicating whether the entries differ by this trait. </summary>
        public override bool DifferenceExists => !string.Equals(LeftFileName, RightFileName,
            CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);

        /// <summary> Initializes comparison from any two entries. </summary>
        /// <param name="left">Left entry.</param>
        /// <param name="right">Right entry.</param>
        /// <returns>true if comparison of two entries by this trait can be performed; false otherwise.</returns>
        protected override bool InitFromEntries(Entry left, Entry right) {
            LeftFileName = left.Path;
            RightFileName = right.Path;
            return true;
        }
    }
}
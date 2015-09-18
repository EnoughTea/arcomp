using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an entry difference by file name. </summary>
    [DataContract(Name = "eFileDiff", IsReference = true, Namespace = "")]
    public class EntryFileNameDifference : EntryTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="ArchiveNameDifference" /> class. </summary>
        /// <param name="left">Left entry.</param>
        /// <param name="right">Right entry.</param>
        public EntryFileNameDifference([CanBeNull] Entry left, [CanBeNull] Entry right)
            : this(left, right, false) {
        }

        /// <summary> Initializes a new instance of the <see cref="ArchiveNameDifference" /> class. </summary>
        /// <param name="left">Left entry.</param>
        /// <param name="right">Right entry.</param>
        /// <param name="caseSensitive">if set to <c>true</c>, file name comparison will be case sensitive.</param>
        public EntryFileNameDifference([CanBeNull] Entry left, [CanBeNull] Entry right, bool caseSensitive)
            : base(left, right) {
            CaseSensitive = caseSensitive;
        }

        /// <summary> Gets the left file name. </summary>
        [DataMember(Name = "lFile", Order = 0)]
        public string LeftFileName { get; private set; }

        /// <summary> Gets the right file name. </summary>
        [DataMember(Name = "rFile", Order = 1)]
        public string RightFileName { get; private set; }

        /// <summary> Gets a value indicating whether filename comparison is case sensitive. </summary>
        [DataMember(Name = "cs", Order = 2)]
        public bool CaseSensitive { get; }

        /// <summary> Gets a value indicating whether the entries differ by this trait. </summary>
        public override bool DifferenceExists => !string.Equals(LeftFileName, RightFileName,
            CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);

        /// <summary> Returns a <see cref="System.String" /> that represents this instance. </summary>
        /// <returns> A <see cref="System.String" /> that represents this instance. </returns>
        public override string ToString() {
            return base.ToString() + $" ({LeftFileName} v {RightFileName})";
        }

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
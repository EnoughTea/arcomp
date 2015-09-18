using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an archive difference by file name. </summary>
    [DataContract(Name = "archiveNameDiff", IsReference = true, Namespace = "")]
    public class ArchiveNameDifference : ArchiveTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="ArchiveNameDifference" /> class. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        public ArchiveNameDifference([CanBeNull] Archive left, [CanBeNull] Archive right)
            : this(left, right, false) {
        }

        /// <summary> Initializes a new instance of the <see cref="ArchiveNameDifference" /> class. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        /// <param name="caseSensitive">if set to <c>true</c>, file name comparison will be case sensitive.</param>
        public ArchiveNameDifference([CanBeNull] Archive left, [CanBeNull] Archive right, bool caseSensitive)
            : base(left, right) {
            CaseSensitive = caseSensitive;
        }

        /// <summary> Gets the left file name. </summary>
        [DataMember(Name = "lName", Order = 0)]
        public string LeftName { get; private set; }

        /// <summary> Gets the right file name. </summary>
        [DataMember(Name = "rName", Order = 1)]
        public string RightName { get; private set; }

        /// <summary> Gets a value indicating whether filename comparison is case sensitive. </summary>
        [DataMember(Name = "cs", Order = 2)]
        public bool CaseSensitive { get; }

        /// <summary> Gets a value indicating whether the archives differ by this trait. </summary>
        public override bool DifferenceExists => !string.Equals(LeftName, RightName,
            CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);

        /// <summary> Returns a <see cref="System.String" /> that represents this instance. </summary>
        /// <returns> A <see cref="System.String" /> that represents this instance. </returns>
        public override string ToString() {
            return base.ToString() + $" ({LeftName} v {RightName})";
        }

        /// <summary> Initializes comparison from any two archives. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        /// <returns>true if comparison of any two archives by this trait can be performed; false otherwise.</returns>
        protected override bool InitFromArchives(Archive left, Archive right) {
            LeftName = left.Name;
            RightName = right.Name;
            return true;
        }
    }
}
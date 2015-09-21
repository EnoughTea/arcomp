using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an archive difference by file name. </summary>
    [DataContract(Name = "archiveNameDiff", IsReference = true, Namespace = "")]
    public class ArchiveFileNameDifference : ArchiveTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="ArchiveFileNameDifference" /> class. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        public ArchiveFileNameDifference([CanBeNull] Archive left, [CanBeNull] Archive right)
            : this(left, right, true) {
        }

        /// <summary> Initializes a new instance of the <see cref="ArchiveFileNameDifference" /> class. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        /// <param name="caseInsensitive">if set to <c>true</c>, file names are considired case insensitive.</param>
        public ArchiveFileNameDifference([CanBeNull] Archive left, [CanBeNull] Archive right, bool caseInsensitive)
            : base(left, right) {
            CaseInsensitive = caseInsensitive;
        }

        /// <summary> Gets the left file name. </summary>
        [DataMember(Name = "lFile", Order = 0)]
        public string LeftFileName { get; private set; }

        /// <summary> Gets the right file name. </summary>
        [DataMember(Name = "rFile", Order = 1)]
        public string RightFileName { get; private set; }

        /// <summary> Gets a value indicating whether filename comparison is case insensitive. </summary>
        [DataMember(Name = "ci", Order = 2)]
        public bool CaseInsensitive { get; }

        /// <summary> Gets a value indicating whether the archives differ by this trait. </summary>
        public override bool DifferenceExists => !string.Equals(LeftFileName, RightFileName,
            CaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString() {
            return base.ToString() + $" ({LeftFileName} v {RightFileName})";
        }

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
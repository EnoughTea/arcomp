using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an entry difference by file path. </summary>
    [DataContract(Name = "eFileDiff", IsReference = true, Namespace = "")]
    public class EntryFilePathDifference : EntryTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="ArchiveFileNameDifference" /> class. </summary>
        /// <param name="left">Left entry.</param>
        /// <param name="right">Right entry.</param>
        public EntryFilePathDifference([CanBeNull] Entry left, [CanBeNull] Entry right)
            : this(left, right, true) {
        }

        /// <summary> Initializes a new instance of the <see cref="ArchiveFileNameDifference" /> class. </summary>
        /// <param name="left">Left entry.</param>
        /// <param name="right">Right entry.</param>
        /// <param name="caseInsensitive">if set to <c>true</c>, file names are considired case insensitive.</param>
        public EntryFilePathDifference([CanBeNull] Entry left, [CanBeNull] Entry right, bool caseInsensitive)
            : base(left, right) {
            CaseInsensitive = caseInsensitive;
        }

        /// <summary> Gets the left file name. </summary>
        [DataMember(Name = "lFile", Order = 0)]
        public string LeftFile { get; private set; }

        /// <summary> Gets the right file name. </summary>
        [DataMember(Name = "rFile", Order = 1)]
        public string RightFile { get; private set; }

        /// <summary> Gets a value indicating whether filename comparison is case insensitive. </summary>
        [DataMember(Name = "ci", Order = 2)]
        public bool CaseInsensitive { get; }

        /// <summary> Gets a value indicating whether the entries differ by this trait. </summary>
        public override bool DifferenceExists => !Entry.IsHomonymousPath(LeftFile, RightFile, CaseInsensitive);

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString() {
            return base.ToString() + $" ({LeftFile} v {RightFile})";
        }

        /// <summary> Initializes comparison from any two entries. </summary>
        /// <param name="left">Left entry.</param>
        /// <param name="right">Right entry.</param>
        /// <returns>true if comparison of two entries by this trait can be performed; false otherwise.</returns>
        protected override bool InitFromEntries(Entry left, Entry right) {
            LeftFile = left.Path;
            RightFile = right.Path;
            return true;
        }
    }
}
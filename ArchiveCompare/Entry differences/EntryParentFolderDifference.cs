using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an entry difference by the parent folder entry. </summary>
    [DataContract(Name = "eParentDiff", IsReference = true, Namespace = "")]
    public class EntryParentFolderDifference : EntryTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="EntryParentFolderDifference" /> class. </summary>
        /// <param name="left">Left entry.</param>
        /// <param name="right">Right entry.</param>
        public EntryParentFolderDifference([CanBeNull] Entry left, [CanBeNull] Entry right)
            : base(left, right) {
        }

        /// <summary> Gets the left parent folder entry. </summary>
        [DataMember(Name = "lParent", Order = 0)]
        public FolderEntry LeftParent { get; private set; }

        /// <summary> Gets the right parent folder entry. </summary>
        [DataMember(Name = "rParent", Order = 1)]
        public FolderEntry RightParent { get; private set; }

        /// <summary> Gets a value indicating whether the entries differ by this trait. </summary>
        public override bool DifferenceExists => LeftParent != RightParent;

        /// <summary> Returns a <see cref="System.String" /> that represents this instance. </summary>
        /// <returns> A <see cref="System.String" /> that represents this instance. </returns>
        public override string ToString() {
            return base.ToString() + $" ({LeftParent.Path} v {RightParent.Path})";
        }

        /// <summary> Initializes comparison from any two entries. </summary>
        /// <param name="left">Left entry.</param>
        /// <param name="right">Right entry.</param>
        /// <returns>true if comparison of two entries by this trait can be performed; false otherwise.</returns>
        protected override bool InitFromEntries(Entry left, Entry right) {
            LeftParent = left.ParentFolder;
            RightParent = right.ParentFolder;
            return true;
        }
    }
}
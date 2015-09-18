using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an entry difference by folder count. </summary>
    [DataContract(Name = "eFoldersDiff", IsReference = true, Namespace = "")]
    public class EntryFolderCountDifference : EntryTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="EntryFolderCountDifference" /> class. </summary>
        /// <param name="left">Left entry.</param>
        /// <param name="right">Right entry.</param>
        public EntryFolderCountDifference([CanBeNull] Entry left, [CanBeNull] Entry right)
            : base(left, right) {
        }

        /// <summary> Gets the left folder count. </summary>
        [DataMember(Name = "lFolders", Order = 0)]
        public int LeftFolderCount { get; private set; }

        /// <summary> Gets the right folder count. </summary>
        [DataMember(Name = "rFolders", Order = 1)]
        public int RightFolderCount { get; private set; }

        /// <summary> Gets a value indicating whether the entries differ by this trait. </summary>
        public override bool DifferenceExists => LeftFolderCount != RightFolderCount;

        /// <summary> Returns a <see cref="System.String" /> that represents this instance. </summary>
        /// <returns> A <see cref="System.String" /> that represents this instance. </returns>
        public override string ToString() {
            return base.ToString() + $" ({LeftFolderCount} v {RightFolderCount})";
        }

        /// <summary> Initializes comparison from two folders. </summary>
        /// <param name="left">Left folder.</param>
        /// <param name="right">Right folder.</param>
        /// <returns>true if comparison of two folders by this trait can be performed; false otherwise.</returns>
        protected override bool InitFromFolders(FolderEntry left, FolderEntry right) {
            LeftFolderCount = left.FolderCount;
            RightFolderCount = right.FolderCount;
            return true;
        }
    }
}
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an entry difference by file count. </summary>
    [DataContract(Name = "eFilesDiff", IsReference = true, Namespace = "")]
    public class EntryFileCountDifference : EntryTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="EntryFileCountDifference" /> class. </summary>
        /// <param name="left">Left entry.</param>
        /// <param name="right">Right entry.</param>
        public EntryFileCountDifference([CanBeNull] Entry left, [CanBeNull] Entry right)
            : base(left, right) {
        }

        /// <summary> Gets the left file count. </summary>
        [DataMember(Name = "lFiles", Order = 0)]
        public int LeftFileCount { get; private set; }

        /// <summary> Gets the right file count. </summary>
        [DataMember(Name = "rFiles", Order = 1)]
        public int RightFileCount { get; private set; }

        /// <summary> Gets a value indicating whether the entries differ by this trait. </summary>
        public override bool DifferenceExists => LeftFileCount != RightFileCount;

        /// <summary> Returns a <see cref="System.String" /> that represents this instance. </summary>
        /// <returns> A <see cref="System.String" /> that represents this instance. </returns>
        public override string ToString() {
            return base.ToString() + $" ({LeftFileCount} v {RightFileCount})";
        }

        /// <summary> Initializes comparison from two folders. </summary>
        /// <param name="left">Left folder.</param>
        /// <param name="right">Right folder.</param>
        /// <returns>true if comparison of two folders by this trait can be performed; false otherwise.</returns>
        protected override bool InitFromFolders(FolderEntry left, FolderEntry right) {
            LeftFileCount = left.FileCount;
            RightFileCount = right.FileCount;
            return true;
        }
    }
}
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an entry difference by file count. </summary>
    public class EntryFileCountDifference : EntryTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="EntryFileCountDifference" /> class. </summary>
        /// <param name="left">Left entry.</param>
        /// <param name="right">Right entry.</param>
        public EntryFileCountDifference([CanBeNull] Entry left, [CanBeNull] Entry right)
            : base(left, right) {
        }

        /// <summary> Gets the left file count. </summary>
        public int LeftFileCount { get; private set; }

        /// <summary> Gets the right file count. </summary>
        public int RightFileCount { get; private set; }

        /// <summary> Gets a value indicating whether the entries differ by this trait. </summary>
        public override bool DifferenceExists => LeftFileCount != RightFileCount;

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
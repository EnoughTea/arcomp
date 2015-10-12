using System.Globalization;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an entry difference by a hash. </summary>
    [DataContract(Name = "eHashDiff", IsReference = true, Namespace = "")]
    public class EntryHashDifference : EntryTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="EntryHashDifference" /> class. </summary>
        /// <param name="left">Left entry.</param>
        /// <param name="right">Right entry.</param>
        public EntryHashDifference([CanBeNull] Entry left, [CanBeNull] Entry right)
            : base(left, right) {
        }

        /// <summary> Gets the left hash. </summary>
        [DataMember(Name = "lHash", Order = 0)]
        public long LeftHash { get; private set; }

        /// <summary> Gets the right hash. </summary>
        [DataMember(Name = "rHash", Order = 1)]
        public long RightHash { get; private set; }

        /// <summary> Gets a value indicating whether the entries differ by this trait. </summary>
        public override bool DifferenceExists => LeftHash != RightHash;

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString() {
            string leftHash = (LeftHash != FileEntry.NoHash)
                ? LeftHash.ToString("X16", CultureInfo.InvariantCulture)
                : "n/a";
            string rightHash = (RightHash != FileEntry.NoHash)
                ? RightHash.ToString("X16", CultureInfo.InvariantCulture)
                : "n/a";
            return base.ToString() + $" ({leftHash} v {rightHash})";
        }

        /// <summary> Initializes comparison from two files. </summary>
        /// <param name="left">Left file.</param>
        /// <param name="right">Right file.</param>
        /// <returns>true if comparison of two files by this trait can be performed; false otherwise.</returns>
        protected override bool InitFromFiles(FileEntry left, FileEntry right) {
            LeftHash = left.Hash;
            RightHash = right.Hash;
            return true;
        }

        /// <summary> Initializes comparison from file and folder entries. </summary>
        /// <param name="left">Left file.</param>
        /// <param name="right">Right folder.</param>
        /// <returns>true if comparison of file and folder entries by this trait can be performed;
        ///  false otherwise.</returns>
        protected override bool InitFromFileAndFolder(FileEntry left, FolderEntry right) {
            LeftHash = left.Hash;
            RightHash = FileEntry.NoHash;
            return true;
        }

        /// <summary> Initializes comparison from folder and file entries. </summary>
        /// <param name="left">Left folder.</param>
        /// <param name="right">Right file.</param>
        /// <returns>true if comparison of folder and file entries by this trait can be performed;
        ///  false otherwise.</returns>
        protected override bool InitFromFolderAndFile(FolderEntry left, FileEntry right) {
            LeftHash = FileEntry.NoHash;
            RightHash = right.Hash;
            return true;
        }
    }
}
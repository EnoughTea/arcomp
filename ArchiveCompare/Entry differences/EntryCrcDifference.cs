using System.Globalization;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an entry difference by CRC. </summary>
    public class EntryCrcDifference : EntryTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="EntryCrcDifference" /> class. </summary>
        /// <param name="left">Left entry.</param>
        /// <param name="right">Right entry.</param>
        public EntryCrcDifference([CanBeNull] Entry left, [CanBeNull] Entry right)
            : base(left, right) {
        }

        /// <summary> Gets the left CRC. </summary>
        public int LeftCrc { get; private set; }

        /// <summary> Gets the right CRC. </summary>
        public int RightCrc { get; private set; }

        /// <summary> Gets a value indicating whether the entries differ by this trait. </summary>
        public override bool DifferenceExists => LeftCrc != RightCrc;

        /// <summary> Returns a <see cref="System.String" /> that represents this instance. </summary>
        /// <returns> A <see cref="System.String" /> that represents this instance. </returns>
        public override string ToString() {
            string leftCrc = (LeftCrc != FileEntry.NoCrc)
                ? LeftCrc.ToString("X8", CultureInfo.InvariantCulture)
                : "n/a";
            string rightCrc = (RightCrc != FileEntry.NoCrc)
                ? RightCrc.ToString("X8", CultureInfo.InvariantCulture)
                : "n/a";
            return base.ToString() + $" ({leftCrc} v {rightCrc})";
        }

        /// <summary> Initializes comparison from two files. </summary>
        /// <param name="left">Left file.</param>
        /// <param name="right">Right file.</param>
        /// <returns>true if comparison of two files by this trait can be performed; false otherwise.</returns>
        protected override bool InitFromFiles(FileEntry left, FileEntry right) {
            LeftCrc = left.Crc;
            RightCrc = right.Crc;
            return true;
        }

        /// <summary> Initializes comparison from file and folder entries. </summary>
        /// <param name="left">Left file.</param>
        /// <param name="right">Right folder.</param>
        /// <returns>true if comparison of file and folder entries by this trait can be performed;
        ///  false otherwise.</returns>
        protected override bool InitFromFileAndFolder(FileEntry left, FolderEntry right) {
            LeftCrc = left.Crc;
            RightCrc = FileEntry.NoCrc;
            return true;
        }

        /// <summary> Initializes comparison from folder and file entries. </summary>
        /// <param name="left">Left folder.</param>
        /// <param name="right">Right file.</param>
        /// <returns>true if comparison of folder and file entries by this trait can be performed;
        ///  false otherwise.</returns>
        protected override bool InitFromFolderAndFile(FolderEntry left, FileEntry right) {
            LeftCrc = FileEntry.NoCrc;
            RightCrc = right.Crc;
            return true;
        }
    }
}
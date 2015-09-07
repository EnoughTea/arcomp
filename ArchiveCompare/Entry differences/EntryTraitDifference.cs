using System.Diagnostics;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an entry difference by some trait. </summary>
    public abstract class EntryTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="ArchiveTraitDifference"/> class. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        protected EntryTraitDifference([CanBeNull] Entry left, [CanBeNull] Entry right) {
            ComparisonExists = From(left, right);
        }

        /// <summary> Gets a value indicating whether it was possible to compare entries by this trait. </summary>
        public bool ComparisonExists { get; }

        /// <summary> Gets a value indicating whether the entries differ by this trait. </summary>
        public abstract bool DifferenceExists { get; }

        /// <summary> Initializes comparison from any two entries. </summary>
        /// <param name="left">Left entry.</param>
        /// <param name="right">Right entry.</param>
        /// <returns>true if comparison of two entries by this trait can be performed; false otherwise.</returns>
        protected virtual bool InitFromEntries(Entry left, Entry right) {
            return false;
        }

        /// <summary> Initializes comparison from two files. </summary>
        /// <param name="left">Left file.</param>
        /// <param name="right">Right file.</param>
        /// <returns>true if comparison of two files by this trait can be performed; false otherwise.</returns>
        protected virtual bool InitFromFiles(FileEntry left, FileEntry right) {
            return InitFromEntries(left, right);
        }

        /// <summary> Initializes comparison from two folders. </summary>
        /// <param name="left">Left folder.</param>
        /// <param name="right">Right folder.</param>
        /// <returns>true if comparison of two folders by this trait can be performed; false otherwise.</returns>
        protected virtual bool InitFromFolders(FolderEntry left, FolderEntry right) {
            return InitFromEntries(left, right);
        }

        /// <summary> Initializes comparison from file and folder entries. </summary>
        /// <param name="left">Left file.</param>
        /// <param name="right">Right folder.</param>
        /// <returns>true if comparison of file and folder entries by this trait can be performed;
        ///  false otherwise.</returns>
        protected virtual bool InitFromFileAndFolder(FileEntry left, FolderEntry right) {
            return InitFromEntries(left, right);
        }

        /// <summary> Initializes comparison from folder and file entries. </summary>
        /// <param name="left">Left folder.</param>
        /// <param name="right">Right file.</param>
        /// <returns>true if comparison of folder and file entries by this trait can be performed;
        ///  false otherwise.</returns>
        protected virtual bool InitFromFolderAndFile(FolderEntry left, FileEntry right) {
            return InitFromEntries(left, right);
        }

        /// <summary> Compares two entries using appropriate difference-finding methods defined by
        ///  this comparison.</summary>
        /// <param name="left">Left entry.</param>
        /// <param name="right">Right entry.</param>
        /// <returns>true if comparison was possible; false if no suitable method was found for
        ///  passed entry type combination.</returns>
        private bool From([CanBeNull] Entry left, [CanBeNull] Entry right) {
            if (left == null || right == null) { return false; }

            var leftFolder = left as FolderEntry;
            var rightFolder = right as FolderEntry;
            var leftFile = left as FileEntry;
            var rightFile = right as FileEntry;

            bool comparedSuccesfully;
            try {
                if (leftFolder != null) {
                    if (rightFolder != null) {
                        comparedSuccesfully = InitFromFolders(leftFolder, rightFolder);
                    } else {
                        Debug.Assert(rightFile != null);
                        comparedSuccesfully = InitFromFolderAndFile(leftFolder, rightFile);
                    }
                } else {
                    if (rightFolder != null) {
                        Debug.Assert(leftFile != null);
                        comparedSuccesfully = InitFromFileAndFolder(leftFile, rightFolder);
                    } else {
                        Debug.Assert(leftFile != null && rightFile != null);
                        comparedSuccesfully = InitFromFiles(leftFile, rightFile);
                    }
                }
            } catch {
                comparedSuccesfully = false;
            }

            return comparedSuccesfully;
        }
    }
}
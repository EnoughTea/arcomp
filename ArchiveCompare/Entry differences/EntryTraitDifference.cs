using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace ArchiveCompare {
    /// <summary> Represents an entry difference by some trait. </summary>
    [DataContract(Name = "entryTraitDiff", IsReference = true, Namespace = "")]
    public abstract class EntryTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="EntryTraitDifference"/> class. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        protected EntryTraitDifference([CanBeNull] Entry left, [CanBeNull] Entry right) {
            ComparisonExists = From(left, right);
        }

        /// <summary> Gets a value indicating whether it was possible to compare entries by this trait. </summary>
        [DataMember(Name = "cmpExists", Order = 0)]
        public bool ComparisonExists { get; }

        /// <summary> Gets a value indicating whether the entries differ by this trait. </summary>
        [DataMember(Name = "diffExists", Order = 1)]
        public abstract bool DifferenceExists { get; }

        /// <summary> Returns a <see cref="System.String" /> that represents this instance. </summary>
        /// <returns> A <see cref="System.String" /> that represents this instance. </returns>
        public override string ToString() {
            string traitName = TypeNameToString(GetType(), "Entry", "Difference");
            string noDifferencePart = !DifferenceExists ? "no " : string.Empty;
            return ComparisonExists
                ? $"{noDifferencePart}{traitName}"
                : $"undefined entry comparison by {traitName}.";
        }

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

        /// <summary> Converts name of the specified type to the human-readable form. </summary>
        /// <param name="type">Type which name to get.</param>
        /// <param name="prefix">Type name prefix.</param>
        /// <param name="suffix">Type name suffix.</param>
        /// <returns>Human-readable type name.</returns>
        internal static string TypeNameToString(Type type, string prefix, string suffix) {
            Contract.Requires(type != null);
            Contract.Requires(prefix != null);
            Contract.Requires(suffix != null);

            string typeName = type.Name;
            int targetLength = typeName.Length - prefix.Length - suffix.Length;
            Debug.Assert(typeName.Length >= targetLength + prefix.Length);
            string traitName = typeName.Substring(prefix.Length, targetLength);
            traitName = TypeNameSplit.Replace(traitName, " ").ToLowerInvariant();
            return traitName;
        }

        private static readonly Regex TypeNameSplit = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z]) |
            (?<=[^A-Z])(?=[A-Z]) |
            (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);
    }
}
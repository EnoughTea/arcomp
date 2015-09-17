using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents differences between 'left' and 'right' versions of the same entry. </summary>
    public class EntryVersionsComparison {
        /// <summary> Initializes a new instance of the <see cref="EntryVersionsComparison"/> class. </summary>
        /// <param name="leftVersion">The left version.</param>
        /// <param name="rightVersion">The right version.</param>
        /// <exception cref="System.ArgumentNullException">Both sides cannot be null</exception>
        public EntryVersionsComparison([CanBeNull] Entry leftVersion, [CanBeNull] Entry rightVersion) {
            LeftVersion = leftVersion;
            RightVersion = rightVersion;
            var differences = new List<EntryTraitDifference>();
            if (leftVersion != null && rightVersion != null) {
                EntryType = Entry.Type(leftVersion);
                differences.AddRange(Entry.PropertiesDiff(leftVersion, rightVersion));
                State = (differences.Count > 0) ? EntryModificationState.Modified : EntryModificationState.Same;
            } else if (rightVersion != null) {
                EntryType = Entry.Type(rightVersion);
                State = EntryModificationState.Added;
            } else if (leftVersion != null) {
                EntryType = Entry.Type(leftVersion);
                State = EntryModificationState.Removed;
            } else {
                EntryType = EntryType.Unknown;
                State = EntryModificationState.Unknown;
            }

            Differences = differences;
        }

        /// <summary> Gets the 'left' version of the entry. </summary>
        public Entry LeftVersion { get; }

        /// <summary> Gets the 'right' version of the entry. </summary>
        public Entry RightVersion { get; }

        /// <summary> Gets the entry type. </summary>
        public EntryType EntryType { get; }

        /// <summary> Gets the differences in properties beetween two entries. </summary>
        public IEnumerable<EntryTraitDifference> Differences { get; }

        /// <summary>
        ///  Gets the value indicating what type of modification entry suffered between versions.
        /// </summary>
        public EntryModificationState State { get; }

        /// <summary> Returns a <see cref="System.String" /> that represents this instance. </summary>
        /// <returns> A <see cref="System.String" /> that represents this instance. </returns>
        public override string ToString() {
            string entryPath;
            if (LeftVersion != null) {
                entryPath = LeftVersion.Path;
            } else if (RightVersion != null) {
                entryPath = RightVersion.Path;
            } else {
                return "empty entry comparison";
            }

            string entryType = Entry.TypeToString(EntryType);
            string header = $"{State.ToString().ToLowerInvariant()} {entryType} '{entryPath}'";
            string modifications = Differences.Aggregate(":", (acc, diff) => acc + Environment.NewLine +
                diff.ToString());
            return $"{header}{modifications}";
        }
    }
}

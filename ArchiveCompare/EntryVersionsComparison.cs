﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents differences between 'left' and 'right' versions of the archive entry. </summary>
    [DataContract(Name = "entryCmp", IsReference = true, Namespace = "")]
    public class EntryVersionsComparison {
        /// <summary> Initializes a new instance of the <see cref="EntryVersionsComparison"/> class. </summary>
        /// <param name="leftVersion">The left version.</param>
        /// <param name="rightVersion">The right version.</param>
        /// <exception cref="ArgumentNullException">Both sides cannot be null</exception>
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

        /// <summary> Gets the entry type. </summary>
        [DataMember(Name = "type", Order = 0)]
        public EntryType EntryType { get; }

        /// <summary>
        ///  Gets the value indicating what type of modification entry suffered between versions.
        /// </summary>
        [DataMember(Name = "state", Order = 1)]
        public EntryModificationState State { get; }

        /// <summary> Gets the 'left' version of the entry. </summary>
        [CanBeNull, DataMember(Name = "left", Order = 2)]
        public Entry LeftVersion { get; }

        /// <summary> Gets the 'right' version of the entry. </summary>
        [CanBeNull, DataMember(Name = "left", Order = 3)]
        public Entry RightVersion { get; }

        /// <summary> Gets the differences in properties beetween two entries. </summary>
        [NotNull, DataMember(Name = "diff", Order = 10)]
        public IEnumerable<EntryTraitDifference> Differences { get; }

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
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
            string modifications = Differences.Aggregate(string.Empty, (acc, diff) => acc + Environment.NewLine +
                diff.ToString());
            modifications = (modifications.Length > 0) ? ": " + modifications : modifications;
            return $"{header}{modifications}";
        }

        [ContractInvariantMethod]
        private void ObjectInvariant() {
            Contract.Invariant(Differences != null);
        }
    }
}

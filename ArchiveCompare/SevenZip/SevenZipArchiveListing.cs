using System.Collections.Generic;
using System.Diagnostics.Contracts;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Plain data holder for 7-Zip output sorter. </summary>
    internal class SevenZipArchiveListing {
        /// <summary> Contains archive properties. </summary>
        public Dictionary<string, string> Properties { get; set; }

        /// <summary> Contains nested archive properties. Null if this is not a split archive. </summary>
        [CanBeNull]
        public Dictionary<string, string> NestedArchiveProperties { get; set; }

        /// <summary> Contains entry lines in simple format. Null if complex format is used. </summary>
        [CanBeNull]
        public string[] SimpleEntries { get; set; }

        /// <summary> Contains entry lines in complex format. Null if simple format is used. </summary>
        [CanBeNull]
        public string[] ComplexEntries { get; set; }

        /// <summary> Gets a value indicating whether the simple entry lines format is used. </summary>
        public bool IsSimpleFormat => SimpleEntries != null || ComplexEntries == null;

        /// <summary> Gets a value indicating whether the archive is a split archive. </summary>
        public bool IsSplitArchive => NestedArchiveProperties != null;

        /// <summary> Gets a value indicating whether the archive is empty. </summary>
        public bool IsEmpty => (SimpleEntries == null || SimpleEntries.Length == 0) &&
                               (ComplexEntries == null || ComplexEntries.Length == 0);
    }
}
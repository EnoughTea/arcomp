using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Plain data holder for 7-Zip output sorter. </summary>
    internal class SevenZipArchiveListing {
        /// <summary> Simple data holder for archive properties. </summary>
        public class ArchiveProperties {
            public string Name { get; set; }
            public ArchiveType Type { get; set; }
            public long Size { get; set; }
            public long PhysicalSize { get; set; }
            public long TotalPhysicalSize { get; set; }
        }

        /// <summary> Contains archive properties. </summary>
        public ArchiveProperties Properties { get; set; }

        /// <summary> Contains nested archive properties. Null if this is not a split archive. </summary>
        [CanBeNull]
        public ArchiveProperties NestedProperties { get; set; }

        /// <summary> Contains entry lines in simple format. Null if complex format is used. </summary>
        [CanBeNull]
        public string[] SimpleEntries { get; set; }

        /// <summary> Contains entry lines in complex format. Null if simple format is used. </summary>
        [CanBeNull]
        public string[] ComplexEntries { get; set; }

        /// <summary> Gets a value indicating whether the simple entry lines format is used. </summary>
        public bool IsSimpleFormat => SimpleEntries != null || ComplexEntries == null;

        /// <summary> Gets a value indicating whether the archive is a split archive. </summary>
        public bool IsSplitArchive => NestedProperties != null;

        /// <summary> Gets a value indicating whether the archive is empty. </summary>
        public bool IsEmpty => (SimpleEntries == null || SimpleEntries.Length == 0) &&
                               (ComplexEntries == null || ComplexEntries.Length == 0);

        /// <summary> Constructs typed archive properties from property map. </summary>
        /// <param name="properties">Property map.</param>
        /// <returns>Typed archive properties.</returns>
        /// <exception cref="ArgumentException">Archive properties do not contain path.</exception>
        [NotNull]
        public static ArchiveProperties PropertiesFromMap(Dictionary<string, string> properties) {
            Contract.Requires(properties != null);

            var result = new ArchiveProperties { Name = properties.GetValue("Path") };
            if (string.IsNullOrEmpty(result.Name)) {
                throw new ArgumentException("Archive properties do not contain path.", nameof(properties));
            }

            result.Type = Archive.StringToType(properties.GetValue("Type"));
            result.Size = SevenZipTools.LongFromString(properties.GetValue("Size"));
            result.PhysicalSize = SevenZipTools.LongFromString(properties.GetValue("Physical Size"));
            result.TotalPhysicalSize = SevenZipTools.LongFromString(properties.GetValue("Total Physical Size"));
            return result;
        }
    }
}
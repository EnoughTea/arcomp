using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace ArchiveCompare {
    /// <summary> Known archive types. </summary>
    public enum ArchiveType {
        /// <summary> Unknown archive type. </summary>
        Unknown,
        /// <summary> Split archive, consisting of several volumes. </summary>
        Split,
        /// <summary> bzip2. </summary>
        BZip2,
        /// <summary> gzip. </summary>
        GZip,
        /// <summary> mbr. </summary>
        Mbr,
        /// <summary> Rar. </summary>
        Rar,
        /// <summary> 7z. </summary>
        SevenZip,
        /// <summary> tar. </summary>
        Tar,
        /// <summary> xz. </summary>
        Xz,
        /// <summary> vhd. </summary>
        Vhd,
        /// <summary> zip. </summary>
        Zip
    }

    /// <summary> Base class for all archives. </summary>
    public abstract class Archive {
        /// <summary> Initializes a new instance of the <see cref="Archive" /> class. </summary>
        /// <param name="name">Archive name.</param>
        /// <param name="type">Archive type.</param>
        /// <param name="lastModified">The last modified date either for this archive or for its most
        ///  lately modified file.</param>
        /// <param name="physicalSize">Size of the archive as reported by file system.</param>
        protected Archive(string name, ArchiveType type, DateTime? lastModified = null, long physicalSize = 0) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(physicalSize >= 0);

            Name = name;
            Type = type;
            LastModified = lastModified;
            PhysicalSize = physicalSize;
        }

        /// <summary> Gets or sets the archive file path.</summary>
        public string Name { get; }

        /// <summary> Gets or sets the archive type. </summary>
        public ArchiveType Type { get; }

        /// <summary> Gets or sets size of the archive as reported by file system. </summary>
        public long PhysicalSize { get; }

        /// <summary> Gets or sets the date when archive was last modified.
        ///  Null means modified date in unavailable.</summary>
        public DateTime? LastModified { get;  }

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString() {
            return Name;
        }

        /// <summary> Converts archive type to string representation. </summary>
        /// <param name="type">Archive type.</param>
        /// <returns>String representation of the given archive type.</returns>
        public static string TypeToString(ArchiveType type) {
            string typeName;
            TypesToNames.TryGetValue(type, out typeName);
            return typeName ?? String.Empty;
        }

        /// <summary> Converts archive type string representation to the archive type. </summary>
        /// <param name="typeName">Archive type string representation.</param>
        /// <returns>Archive type corresponding to the given string.</returns>
        public static ArchiveType StringToType(string typeName) {
            ArchiveType archiveType;
            NamesToTypes.TryGetValue(typeName, out archiveType);
            return archiveType;
        }

        private static readonly Dictionary<ArchiveType, string> TypesToNames = new Dictionary<ArchiveType, string> {
            { ArchiveType.BZip2, "bzip2" },
            { ArchiveType.GZip, "gzip" },
            { ArchiveType.Mbr, "mbr" },
            { ArchiveType.Rar, "Rar" },
            { ArchiveType.Tar, "tar" },
            { ArchiveType.Vhd, "vhd" },
            { ArchiveType.Xz, "xz" },
            { ArchiveType.Zip, "zip" },
            { ArchiveType.SevenZip, "7z" },
            { ArchiveType.Split, "Split" },
            { ArchiveType.Unknown, String.Empty }
        };

        private static readonly Dictionary<string, ArchiveType> NamesToTypes = TypesToNames
            .ToDictionary(key => key.Value, value => value.Key);
    }
}
﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;

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
        /// <summary> PE. </summary>
        Pe,
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
        /// <param name="lastModified">The last modified date for this archive latest modified file.</param>
        /// <param name="physicalSize">Size of the archive as reported by file system.</param>
        protected Archive(string name, ArchiveType type, DateTime? lastModified = null, long physicalSize = 0) {
            Contract.Requires(!string.IsNullOrEmpty(name));
            Contract.Requires(physicalSize >= 0);

            Name = name;
            Type = type;
            LastModified = lastModified;
            PhysicalSize = physicalSize;
        }

        private DateTime? _lastModified;

        /// <summary> Gets or sets the archive file path.</summary>
        [NotNull]
        public string Name { get; }

        /// <summary> Gets or sets the archive type. </summary>
        public ArchiveType Type { get; }

        /// <summary> Gets or sets size of the archive as reported by file system. For split archives means
        ///  user-defined part size (last part could be of any size). </summary>
        public long PhysicalSize { get; }

        /// <summary> Gets or sets the last modified date of the latest modified file in the archive.
        ///  Null means latest modified file date in unavailable.</summary>
        public DateTime? LastModified {
            get {
                var split = this as SplitArchive;
                return (split != null && _lastModified == null) ? split.Nested.LastModified : _lastModified;
            }

            internal set { _lastModified = value; }
        }

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString() {
            string lastModified = (LastModified != null) ? ", modified on " + LastModified : string.Empty;
            return Name + lastModified + ", physical size " + PhysicalSize;
        }

        /// <summary> Converts archive type to string representation. </summary>
        /// <param name="type">Archive type.</param>
        /// <returns>String representation of the given archive type.</returns>
        public static string TypeToString(ArchiveType type) {
            Contract.Ensures(Contract.Result<string>() != null);

            return TypesToNames.GetValue(type)?.ToLowerInvariant() ?? string.Empty;
        }

        /// <summary> Converts archive type string representation to the archive type. </summary>
        /// <param name="typeName">Archive type string representation.</param>
        /// <returns>Archive type corresponding to the given string.</returns>
        public static ArchiveType StringToType([CanBeNull] string typeName) {
            return !string.IsNullOrEmpty(typeName)
                ? NamesToTypes.GetValue(typeName.ToLowerInvariant())
                : ArchiveType.Unknown;
        }

        private static readonly Dictionary<ArchiveType, string> TypesToNames = new Dictionary<ArchiveType, string> {
            { ArchiveType.BZip2, "bzip2" },
            { ArchiveType.GZip, "gzip" },
            { ArchiveType.Mbr, "mbr" },
            { ArchiveType.Pe, "pe" },
            { ArchiveType.Rar, "rar" },
            { ArchiveType.Tar, "tar" },
            { ArchiveType.Vhd, "vhd" },
            { ArchiveType.Xz, "xz" },
            { ArchiveType.Zip, "zip" },
            { ArchiveType.SevenZip, "7z" },
            { ArchiveType.Split, "split" },
            { ArchiveType.Unknown, "unknown" }
        };

        private static readonly Dictionary<string, ArchiveType> NamesToTypes = TypesToNames
            .ToDictionary(key => key.Value, value => value.Key);

    }
}
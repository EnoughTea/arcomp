using System;
using System.Diagnostics.Contracts;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents single archive entry. </summary>
    public abstract class Entry {
        /// <summary> Initializes a new instance of the <see cref="Entry" /> class. </summary>
        /// <param name="name">Full file or folder name.</param>
        /// <param name="parent">Parent directory. Null means entry is located at archive's root.</param>
        /// <param name="lastModified">The date when file was last modified.</param>
        /// <param name="size">Uncompressed size. 0 means uncompressed size is unavailable.</param>
        /// <param name="packedSize">Compressed size. 0 means that either compressed size is unavailable or
        ///  no compression was done on the entry.</param>
        protected Entry(string name, FolderEntry parent = null, DateTime? lastModified = null, long size = 0,
            long packedSize = 0) {
            Contract.Requires(!string.IsNullOrEmpty(name));
            Contract.Requires(size >= 0);

            Name = name;
            ParentFolder = parent;
            Size = size;
            PackedSize = packedSize;
            LastModifed = lastModified;
        }

        /// <summary> Gets the archive entry name.</summary>
        [NotNull]
        public string Name { get; }

        /// <summary> Gets or sets the date when file was last modified.
        ///  Null means modified date in unavailable.</summary>
        public DateTime? LastModifed { get; }

        /// <summary> Gets or sets the uncompressed size. 0 means that entry is a folder or
        ///  uncompressed size is unavailable. </summary>
        public long Size { get; }

        /// <summary> Gets or sets the compressed size.
        /// 0 means that either entry is a folder, or no compression was done on the entry,
        /// or compressed size is unavailable.
        /// </summary>
        public long PackedSize { get; }

        /// <summary> Gets or sets the parent directory. Null means root. </summary>
        public FolderEntry ParentFolder { get; set; }

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString() {
            return Name;
        }
    }
}
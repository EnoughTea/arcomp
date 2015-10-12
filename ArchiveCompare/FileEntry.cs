using System;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace ArchiveCompare {
    /// <summary> Represents an archived file. </summary>
    [DataContract(Name = "file", IsReference = true, Namespace = "")]
    public class FileEntry : Entry {
        /// <summary> Initializes a new instance of the <see cref="FileEntry" /> class. </summary>
        /// <param name="path">Full file name.</param>
        /// <param name="lastModified">The date when file was last modified.</param>
        /// <param name="size">Uncompressed size. 0 means uncompressed size is unavailable.</param>
        /// <param name="packedSize">Compressed size. 0 means that either compressed size is unavailable or
        ///     no compression was done on the entry.</param>
        /// <param name="hash">File hash, value equal to <see cref="NoHash"/> means hash in unavailable.</param>
        /// <param name="parent">Parent directory. Null means entry is located at archive's root.</param>
        public FileEntry(string path, DateTime? lastModified = null, long size = 0, long packedSize = 0,
            long hash = NoHash, FolderEntry parent = null)
            : base(path, lastModified,  size, packedSize, parent) {
            Contract.Requires(!string.IsNullOrWhiteSpace(path));
            Contract.Requires(size >= 0);
            Contract.Requires(packedSize >= 0);

            Hash = hash;
        }

        /// <summary> Gets this file's name with extension. </summary>
        public override string Name => System.IO.Path.GetFileName(Path);

        /// <summary> File hash. </summary>
        [DataMember(Name = "hash", IsRequired = false)]
        public long Hash { get; }

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString() {
            return "file " + base.ToString() +", size " + Size + ", packed size " + PackedSize;
        }

        /// <summary> Means hash is unavailable. </summary>
        internal const int NoHash = 0;
    }
}
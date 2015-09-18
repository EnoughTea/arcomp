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
        /// <param name="crc">File checksum, values less than zero means CRC in unavailable.</param>
        /// <param name="parent">Parent directory. Null means entry is located at archive's root.</param>
        public FileEntry(string path, DateTime? lastModified = null, long size = 0, long packedSize = 0,
            int crc = NoCrc, FolderEntry parent = null)
            : base(path, lastModified,  size, packedSize, parent) {
            Contract.Requires(!string.IsNullOrWhiteSpace(path));
            Contract.Requires(size >= 0);
            Contract.Requires(packedSize >= 0);

            Crc = crc;
        }

        /// <summary> Gets this file's name with extension. </summary>
        public override string Name => System.IO.Path.GetFileName(Path);

        /// <summary> File checksum, values less than zero means CRC in unavailable. </summary>
        [DataMember(Name = "crc", IsRequired = false)]
        public int Crc { get; }

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString() {
            return "file " + base.ToString() +", size " + Size + ", packed size " + PackedSize;
        }

        /// <summary> Means CRC is unavailable. </summary>
        internal const int NoCrc = -1;
    }
}
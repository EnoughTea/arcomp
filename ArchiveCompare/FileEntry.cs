using System;
using System.Diagnostics.Contracts;

namespace ArchiveCompare {
    /// <summary> Represents an archived file. </summary>
    public class FileEntry : Entry {
        /// <summary> Initializes a new instance of the <see cref="FileEntry" /> class. </summary>
        /// <param name="name">Full file name.</param>
        /// <param name="lastModified">The date when file was last modified.</param>
        /// <param name="size">Uncompressed size. 0 means uncompressed size is unavailable.</param>
        /// <param name="packedSize">Compressed size. 0 means that either compressed size is unavailable or
        ///     no compression was done on the entry.</param>
        /// <param name="crc">File checksum, values less than zero means CRC in unavailable.</param>
        /// <param name="parent">Parent directory. Null means entry is located at archive's root.</param>
        public FileEntry(string name, DateTime? lastModified = null, long size = 0, long packedSize = 0, int crc = -1,
            FolderEntry parent = null)
            : base(name, lastModified,  size, packedSize, parent) {
            Contract.Requires(size >= 0);
            Contract.Requires(packedSize >= 0);

            Crc = crc;
        }

        /// <summary> File checksum, values less than zero means CRC in unavailable. </summary>
        public int Crc { get; }

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString() {
            return "file " + base.ToString() +", size " + Size + ", packed size " + PackedSize;
        }
    }
}
using System;
using System.Diagnostics.Contracts;
using System.IO;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents one file entry in BSA archive. </summary>
    internal struct BsaFileRecord : IEquatable<BsaFileRecord> {
        /// <summary> Empty entry. </summary>
        public static readonly BsaFileRecord Empty = new BsaFileRecord();

        /// <summary> Initializes a new instance of the <see cref="BsaFileRecord" /> struct. </summary>
        /// <param name="path">Full file name.</param>
        /// <param name="offset">File offset from the beginning of the archive in bytes.</param>
        /// <param name="size">File size in bytes.</param>
        /// <param name="hash">Hash of the file name.</param>
        /// <param name="compressed">if set to <c>true</c>, file is compressed.</param>
        public BsaFileRecord(string path, long offset, long size, long hash, bool compressed) {
            Contract.Requires(path != null);
            Contract.Requires(offset >= 0);
            Contract.Requires(size >= 0);

            Offset = offset;
            Size = size;
            Path = path;
            Hash = hash;
            Compressed = compressed;
        }

        /// <summary> File name. </summary>
        public string Path { get; }

        /// <summary> Folder name. </summary>
        public string FolderPath => !string.IsNullOrWhiteSpace(Path)
            ? System.IO.Path.GetDirectoryName(Path)
            : string.Empty;

        /// <summary> File offset from the beginning of the archive in bytes, not the offset into the data buffer
        /// (which is what is stored in the archive.) </summary>
        public long Offset { get; }

        /// <summary> File size in bytes. </summary>
        public long Size { get; }

        /// <summary> Hash of the file name. </summary>
        public long Hash { get; }

        /// <summary> True if file is compressed; false otherwise. </summary>
        public bool Compressed { get; }

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString() {
            string name = !string.IsNullOrWhiteSpace(Path) ? Path : "<unnamed>";
            return $"{name} ({Size} bytes at {Offset} offset)";
        }

        /// <summary> Indicates whether the current object is equal to another object of the same type. </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(BsaFileRecord other) {
            return string.Equals(Path, other.Path) && Offset == other.Offset && Size == other.Size &&
                Hash == other.Hash && Compressed == other.Compressed;
        }

        /// <summary> Determines whether the specified <see cref="Object" />, is equal to this instance. </summary>
        /// <param name="obj">The <see cref="Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals([CanBeNull] object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is BsaFileRecord && Equals((BsaFileRecord)obj);
        }

        /// <summary> Returns a hash code for this instance. </summary>
        /// <returns> A hash code for this instance, suitable for use in hashing algorithms and data structures like
        ///  a hash table. </returns>
        public override int GetHashCode() {
            unchecked {
                var hashCode = Path?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ Offset.GetHashCode();
                hashCode = (hashCode * 397) ^ Size.GetHashCode();
                hashCode = (hashCode * 397) ^ Hash.GetHashCode();
                hashCode = (hashCode * 397) ^ Compressed.GetHashCode();
                return hashCode;
            }
        }

        /// <summary> Implements the operator ==. </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns> true if left is equal to the right; false otherwise. </returns>
        public static bool operator ==(BsaFileRecord left, BsaFileRecord right) {
            return left.Equals(right);
        }

        /// <summary> Implements the operator !=. </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns> true if left is not equal to the right; false otherwise. </returns>
        public static bool operator !=(BsaFileRecord left, BsaFileRecord right) {
            return !left.Equals(right);
        }
    }
}

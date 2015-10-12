using System;
using System.Diagnostics.Contracts;

namespace ArchiveCompare {
    /// <summary> Represents one file entry in BSA archive. </summary>
    internal struct BsaFolderRecord : IEquatable<BsaFolderRecord> {
        /// <summary> Empty entry. </summary>
        public static readonly BsaFolderRecord Empty = new BsaFolderRecord();

        /// <summary> Initializes a new instance of the <see cref="BsaFolderRecord" /> struct. </summary>
        /// <param name="path">Folder name.</param>
        /// <param name="offset">Offset to name of this folder + TotalFileNameLength.</param>
        /// <param name="count">Amount of files in this folder.</param>
        /// <param name="hash">Hash of the folder name.</param>
        public BsaFolderRecord(string path, long offset, long count, long hash) {
            Contract.Requires(path != null);
            Contract.Requires(offset >= 0);
            Contract.Requires(count >= 0);

            Offset = offset;
            Count = count;
            Path = path;
            Hash = hash;
        }

        /// <summary> Folder name. </summary>
        public string Path { get; }

        /// <summary> Offset to name of this folder + TotalFileNameLength. </summary>
        public long Offset { get; }

        /// <summary>Amount of files in this folder.</summary>
        public long Count { get; }

        /// <summary>Hash of the folder name.</summary>
        public long Hash { get; }

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString() {
            string name = !string.IsNullOrWhiteSpace(Path) ? Path : "<unnamed>";
            return $"folder '{name}' ({Count} files at {Offset} offset)";
        }

        /// <summary> Indicates whether the current object is equal to another object of the same type. </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(BsaFolderRecord other) {
            return string.Equals(Path, other.Path) && Offset == other.Offset && Count == other.Count &&
                Hash == other.Hash;
        }

        /// <summary> Determines whether the specified <see cref="Object" />, is equal to this instance. </summary>
        /// <param name="obj">The <see cref="Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is BsaFolderRecord && Equals((BsaFolderRecord)obj);
        }

        /// <summary> Returns a hash code for this instance. </summary>
        /// <returns> A hash code for this instance, suitable for use in hashing algorithms and data structures
        ///  like a hash table. </returns>
        public override int GetHashCode() {
            unchecked {
                var hashCode = Path?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ Offset.GetHashCode();
                hashCode = (hashCode * 397) ^ Count.GetHashCode();
                hashCode = (hashCode * 397) ^ Hash.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(BsaFolderRecord left, BsaFolderRecord right) {
            return left.Equals(right);
        }

        public static bool operator !=(BsaFolderRecord left, BsaFolderRecord right) {
            return !left.Equals(right);
        }
    }
}
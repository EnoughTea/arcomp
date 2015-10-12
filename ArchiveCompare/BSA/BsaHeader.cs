using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace ArchiveCompare {
    /// <summary> Flags defining implementation details. </summary>
    [Flags]
    internal enum BsaOptions {
        Unknown = 0,
        IncludeDirectoryNames = 0x1,
        IncludeFileNames = 0x2,
        CompressedArchive = 0x4,
        RetainDirectoryNames = 0x8,
        RetainFileNames = 0x10,
        RetainFileNameOffsets = 0x20,
        /// <summary> Hash values and numbers after the header are encoded big-endian. </summary>
        Xbox360Archive = 0x40,
        RetainStringsDuringStartup = 0x80,
        /// <summary>Indicates the file data blocks begin with the full path of the file.</summary>
        EmbedFileNames = 0x100,
        /// <summary> This can only be used with Bit 3 (Compress Archive).
        ///  This is an Xbox 360 only compression algorithm. </summary>
        XMemCodec = 0x200,
        Unused = 0x400
    }

    /// <summary> Flags describing BSA contents. </summary>
    [Flags]
    internal enum BsaContents {
        Unknown = 0,
        Meshes = 0x1,
        Textures = 0x2,
        Menus = 0x4,
        Sounds = 0x8,
        Voices = 0x10,
        Shaders = 0x20,
        Trees = 0x40,
        Fonts = 0x80,
        Miscellaneous = 0x100
    }

    /// <summary> Represents generic BSA header. </summary>
    internal sealed class BsaHeader {
        /// <summary> Gets the header size in bytes. </summary>
        public int Size => MorrowindFormat ? 12 : 36;

        /// <summary> Gets the BSA version. </summary>
        public uint Version { get; private set; }

        /// <summary> Gets the offset for the beginning of folder records.
        ///  For Morrowind format it is size/offset-table + filename sections. </summary>
        public uint Offset { get; private set; }

        /// <summary> Gets the archive flags. </summary>
        public BsaOptions ArchiveFlags { get; private set; }

        /// <summary> Gets the count of all folders in archive. </summary>
        public uint FolderCount { get; private set; }

        /// <summary> Gets the count of all files in archive. </summary>
        public uint FileCount { get; private set; }

        /// <summary> Gets the total length of all folder names,
        ///  including \0's but not including the prefixed length byte. </summary>
        public uint TotalFolderNameLength { get; private set; }

        /// <summary> Gets the total length of all file names, including \0's. </summary>
        public uint TotalFileNameLength { get; private set; }

        /// <summary> Gets the file flags for this archive. </summary>
        public BsaContents FileFlags { get; private set; }

        /// <summary> Gets or sets a value indicating whether the header belongs to a TES3 archive. </summary>
        public bool MorrowindFormat { get; private set; }

        /// <summary> Gets a value indicating whether this is a valid header. </summary>
        public bool Valid { get; private set; }

        /// <summary> Reads BSA header from the specified stream. </summary>
        /// <param name="reader">BSA data stream reader.</param>
        /// <returns>true if stream data contained a BSA header; false otherwise.</returns>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        /// <exception cref="NotSupportedException"> Stream does not support seeking. </exception>
        public bool Read(BinaryReader reader) {
            MorrowindFormat = IsLegacyBsa(reader);

            long initialPos = reader.BaseStream.Position;
            long totalSize = reader.BaseStream.Length - initialPos;
            if (totalSize < Size) {
                return false; // "File is too small to be a valid BSA archive."
            }

            Valid = ActualRead(reader);
            if (!Valid) {
                reader.BaseStream.Position = initialPos;
            }

            return Valid;
        }

        private bool ActualRead(BinaryReader reader) {
            ulong bsaSizeExceptHeader = (ulong)(reader.BaseStream.Length - Size);
            // Try to read stream as legacy BSA first:
            if (MorrowindFormat) {
                uint[] head = reader.ReadUints(3).ToArray();
                Contract.Assume(head.Length == 3);
                Version = head[0];
                // Head contains total number of bytes used in size/offset-table + filename sections, and file count.
                Offset = head[1];
                FileCount = head[2];
                ulong fileRecordSize = FileCount * 12;
                TotalFileNameLength = (uint)(Offset - fileRecordSize);
                ulong minimumBsaSize = FileCount * 21;
                ulong hashTableSize = 8 * FileCount;
                if ((minimumBsaSize > bsaSizeExceptHeader) || (Offset + hashTableSize > bsaSizeExceptHeader)) {
                    return false; // "Directory information larger than entire archive."
                }

                return true;
            }

            // Now try to read stream as a modern BSA:
            byte[] fileId;
            try {
                fileId = reader.ReadBytes(4);
                Version = reader.ReadUInt32();
                Offset = reader.ReadUInt32();
                ArchiveFlags = (BsaOptions)reader.ReadUInt32();
                FolderCount = reader.ReadUInt32();
                FileCount = reader.ReadUInt32();
                TotalFolderNameLength = reader.ReadUInt32();
                TotalFileNameLength = reader.ReadUInt32();
                FileFlags = (BsaContents)reader.ReadUInt32();
                ulong minimumBsaSize = FileCount * 16 + FolderCount * 16;
                if (minimumBsaSize > bsaSizeExceptHeader) {
                    return false; // "Record information larger than entire archive."
                }

            } catch {
                return false;
            }

            Contract.Assume(fileId.Length == 4);
            if (fileId[0] != 'B' || fileId[1] != 'S' || fileId[2] != 'A' || fileId[3] != 0 ||
                (Version != 0x67 && Version != 0x68)) {
                return false; // "Unrecognized BSA header."
            }

            return true;
        }

        private static bool IsLegacyBsa(BinaryReader reader) {
            long initialPos = reader.BaseStream.Position;
            uint[] head;
            try {
                head = reader.ReadUints(3);
            } catch {
                return false;
            } finally {
                reader.BaseStream.Position = initialPos;
            }

            return head[0] == 0x100;
        }
    }
}

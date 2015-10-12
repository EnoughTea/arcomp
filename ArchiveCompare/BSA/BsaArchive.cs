using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

namespace ArchiveCompare {
    /// <summary> Represents BSA data read from a file stream. </summary>
    internal sealed class BsaArchive {
        /// <summary> Gets the BSA header. </summary>
        public BsaHeader Header { get; }

        /// <summary> Gets records for all files contained within the BSA. </summary>
        public List<BsaFileRecord> Files { get; }

        /// <summary> Gets records for all folders containted within the BSA. </summary>
        public List<BsaFolderRecord> Folders { get; }

        public long DataOffset { get; }

        /// <exception cref="ArgumentException">The stream does not support reading, is null,
        /// or is already closed. </exception>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        /// <exception cref="NotSupportedException"> Stream does not support seeking. </exception>
        public BsaArchive(Stream bsaData) {
            Contract.Requires(bsaData != null);

            var reader = new BinaryReader(bsaData);
            Header = new BsaHeader();
            Files = new List<BsaFileRecord>();
            Folders = new List<BsaFolderRecord>();

            if (!Header.Read(reader)) { return; }

            long bytesBeforeData = Header.MorrowindFormat ? ReadLegacyData(reader) : ReadModernData(reader);
            DataOffset = Header.Size + bytesBeforeData;
        }

        private long ReadLegacyData(BinaryReader reader) {
            Contract.Requires(reader != null);

            long initialPos = reader.BaseStream.Position;
            long directorySize = Header.Offset;
            long fileCount = Header.FileCount;
            long hashTableSize = fileCount * 8;

            uint[] fileDataHeaders = reader.ReadUints(2 * fileCount);
            uint[] nameOffsets = reader.ReadUints(fileCount);
            byte[] nameBuffer = reader.ReadBytes((int)Header.TotalFileNameLength);
            ulong[] hashes = reader.ReadUlongs(fileCount);

            // Calculate the offset of the data buffer. All file offsets are relative to this.
            long fileDataOffset = Header.Size + directorySize + hashTableSize;
            for (int i = 0; i < fileCount; i++) {
                string path = NullTerminatedStringFromBytes(nameBuffer, (int)nameOffsets[i]);
                long size = fileDataHeaders[i * 2];
                long offset = fileDataHeaders[i * 2 + 1] + fileDataOffset;
                long hash = (long)hashes[i];

                Files.Add(new BsaFileRecord(path, offset, size, hash, false));
            }

            var allFolders = from file in Files
                where file.FolderPath != string.Empty
                select file.FolderPath;
            foreach (var folderPath in allFolders.Distinct()) {
                Folders.Add(new BsaFolderRecord(folderPath, 0, 0, 0));
            }

            return reader.BaseStream.Position - initialPos;
        }


        private long ReadModernData(BinaryReader reader) {
            Contract.Requires(reader != null);

            long initialPos = reader.BaseStream.Position;
            bool hasFileNames =
                (Header.ArchiveFlags & BsaOptions.IncludeFileNames) == BsaOptions.IncludeFileNames;
            bool hasDirNames =
                (Header.ArchiveFlags & BsaOptions.IncludeDirectoryNames) == BsaOptions.IncludeDirectoryNames;
            bool defaultCompressed =
                (Header.ArchiveFlags & BsaOptions.CompressedArchive) == BsaOptions.CompressedArchive;

            for (int initial = 0; initial < Header.FolderCount; initial++) {
                ulong hash = reader.ReadUInt64();
                uint count = reader.ReadUInt32();
                uint offset = reader.ReadUInt32();
                Folders.Add(new BsaFolderRecord(string.Empty, offset, count, (long)hash));
            }

            for (int folderIndex = 0; folderIndex < Header.FolderCount; folderIndex++) {
                if (hasDirNames) {
                    int length = reader.ReadByte();
                    string folderName = NullTerminatedStringFromBytes(reader.ReadBytes(length), 0);
                    if (!string.IsNullOrWhiteSpace(folderName)) {
                        var oldFolder = Folders[folderIndex];
                        Folders[folderIndex] = new BsaFolderRecord(folderName, oldFolder.Offset, oldFolder.Count,
                            oldFolder.Hash);
                    }
                }

                var folder = Folders[folderIndex];
                int fileCount = (int)folder.Count;
                for (int fileIndex = 0; fileIndex < fileCount; fileIndex++) {
                    ulong hash = reader.ReadUInt64();
                    uint size = reader.ReadUInt32();
                    uint offset = reader.ReadUInt32();
                    bool compressed = defaultCompressed;
                    // If the (1 << 30) bit is set in the size:
                    //   * If files are default compressed, this file is not compressed.
                    //   * If files are default not compressed, this file is compressed.
                    // If the file is compressed, the file data will have the specification of compressed file block.
                    // In addition, the size of compressed data is considered to be (4 bytes + compressed size).
                    if ((size & 0x40000000) > 0) {
                        compressed = !compressed;
                        size ^= 0x40000000;
                    }

                    string combinedFileName = (folder.Path != string.Empty)
                        ? folder.Path + "\\<unnamed>"
                        : "<unnamed>";
                    Files.Add(new BsaFileRecord(combinedFileName, offset, size, (long)hash, compressed));
                }
            }

            if (hasFileNames) {
                for (int fIndex = 0; fIndex < Header.FileCount; fIndex++) {
                    string fileName = reader.ReadNullTerminatedString();
                    if (!string.IsNullOrWhiteSpace(fileName)) {
                        var oldFile = Files[fIndex];
                        string newFileName = (oldFile.FolderPath != string.Empty)
                            ? oldFile.FolderPath + "\\" + fileName
                            : fileName;
                        Files[fIndex] = new BsaFileRecord(newFileName, oldFile.Offset, oldFile.Size,
                            oldFile.Hash, oldFile.Compressed);
                    }
                }
            }

            return reader.BaseStream.Position - initialPos;
        }

        private static string NullTerminatedStringFromBytes(byte[] nameBuffer, int nameStartPos) {
            Contract.Requires(nameBuffer != null);
            Contract.Requires(nameStartPos >= 0);

            string name = string.Empty;
            int nullTerminatorPos = Array.FindIndex(nameBuffer, nameStartPos, c => c == 0);
            if (nullTerminatorPos < 0) { nullTerminatorPos = nameBuffer.Length; }

            int length = nullTerminatorPos - nameStartPos;
            if (length > 0) { name = Encoding.UTF8.GetString(nameBuffer, nameStartPos, length); }

            return name;
        }
    }
}
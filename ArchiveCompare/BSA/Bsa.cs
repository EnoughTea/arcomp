using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace ArchiveCompare {
    // Thanks to https://github.com/OpenMW/openmw and http://www.uesp.net/wiki/ for info on BSAs.

    /// <summary> Provides helper methods for archive creation from BSA (Bethesda Archive) file. </summary>
    public static class Bsa {
        /// <summary> Creates a single archive from a BSA data stream. </summary>
        /// <param name="path">Full path of the BSA archive.</param>
        /// <param name="lastModified">'Last modified' of the BSA file.</param>
        /// <param name="bsaData">BSA data stream.</param>
        /// <returns>Created archive or null.</returns>
        [CanBeNull]
        public static SingleArchive ArchiveFromStream(string path, DateTime? lastModified, Stream bsaData) {
            Contract.Requires(!string.IsNullOrWhiteSpace(path));
            Contract.Requires(bsaData != null);

            BsaArchive bsaArchive;
            try {
                bsaArchive = new BsaArchive(bsaData);
            } catch { return null; }

            var folders = bsaArchive.Folders.Where(f => !string.IsNullOrWhiteSpace(f.Path))
                .Select(f => new FolderEntry(f.Path));
            var files = bsaArchive.Files.Select(bsaEntry => new FileEntry(bsaEntry.Path, null, bsaEntry.Size, 0,
                bsaEntry.Hash));
            return new SingleArchive(path, ArchiveType.Bsa, folders.Cast<Entry>().Concat(files), bsaData.Length,
                bsaData.Length, 0, lastModified);
        }
    }
}

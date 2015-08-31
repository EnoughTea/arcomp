using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Provides helper methods for archive creation from 7-Zip console output. </summary>
    public static class SevenZip {
        /// <summary> Creates archives from 7-Zip console output, created by 'l -slt' command. </summary>
        /// <param name="sevenZipConsoleListingOutput">The 7-Zip 'l -slt' command output.</param>
        /// <returns> Created archives. </returns>
        [NotNull]
        public static IEnumerable<Archive> CreateFromOutput(string sevenZipConsoleListingOutput) {
            var metadatas = SevenZipArchiveMetadata.ParseMany(sevenZipConsoleListingOutput);
            foreach (var metadata in metadatas) {
                Archive archive = null;
                try { archive = metadata.ToArchive(); } catch (InvalidOperationException) { }

                if (archive != null) { yield return archive; }
            }
        }
    }
}

using System.Collections.Generic;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Provides helper methods for archive creation from 7-Zip console output. </summary>
    public static class SevenZip {
        /// <summary> Creates archives from 7-Zip console output, created by 'l -slt' command. </summary>
        /// <param name="sevenZipConsoleListingOutput">The 7-Zip 'l -slt' command output.</param>
        /// <returns> Created archives. </returns>
        [NotNull]
        public static IEnumerable<Archive> ArchivesFromOutput(string sevenZipConsoleListingOutput) {
            return SevenZipParser.ParseArchives(sevenZipConsoleListingOutput);
        }
    }
}

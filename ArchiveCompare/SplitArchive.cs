﻿using System;
using System.Diagnostics.Contracts;

namespace ArchiveCompare {
    /// <summary> Archive split over several files. </summary>
    public class SplitArchive : Archive {
        /// <summary>
        /// Initializes a new instance of the <see cref="SplitArchive" /> class.
        /// </summary>
        /// <param name="name">Archive name.</param>
        /// <param name="nested">Nested single archive.</param>
        /// <param name="lastModified">The last modified date either for this archive or for its most
        ///  lately modified file.</param>
        /// <param name="physicalSize">Size of the archive part as reported by file system.</param>
        /// <param name="totalPhysicalSize">Total size of all parts as reported by file system.</param>
        public SplitArchive(string name, SingleArchive nested, DateTime? lastModified = null, long physicalSize = 0,
            long totalPhysicalSize = 0)
            : base(name, ArchiveType.Split, lastModified, physicalSize) {
            Contract.Requires(nested != null);

            Nested = nested;
            TotalPhysicalSize = totalPhysicalSize;
        }

        /// <summary> Gets the nested archive. </summary>
        public SingleArchive Nested { get; }

        /// <summary> Gets total size of the archive as reported by file system. </summary>
        public long TotalPhysicalSize { get; }

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString() {
            return "split archive '" + base.ToString() + "'";
        }
    }
}
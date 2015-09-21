using System;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Archive split over several files. </summary>
    [DataContract(Name = "splitArchive", IsReference = true, Namespace = "")]
    public class SplitArchive : Archive {
        [DataMember(Name = "totalPhysSize", IsRequired = false, Order = 0)]
        private readonly long _totalPhysicalSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitArchive" /> class.
        /// </summary>
        /// <param name="path">Archive name.</param>
        /// <param name="nested">Nested single archive.</param>
        /// <param name="physicalSize">Size of the archive part as reported by file system.</param>
        /// <param name="totalPhysicalSize">Total size of all parts as reported by file system.</param>
        /// <param name="lastModified">The last modified date for this archive latest modified file.</param>
        public SplitArchive(string path, SingleArchive nested, long physicalSize = 0,
            long totalPhysicalSize = 0, DateTime? lastModified = null)
            : base(path, ArchiveType.Split, physicalSize, lastModified) {
            Contract.Requires(nested != null);
            Contract.Requires(physicalSize >= 0);
            Contract.Requires(totalPhysicalSize >= 0);

            Nested = nested;
            _totalPhysicalSize = totalPhysicalSize;
        }

        /// <summary> Gets the nested archive. </summary>
        [NotNull, DataMember(Name = "nestedArchive", IsRequired = true, Order = 10)]
        public SingleArchive Nested { get; }

        /// <summary> Gets total size of the archive as reported by file system. </summary>
        public long TotalPhysicalSize => (_totalPhysicalSize == 0) ? Nested.PhysicalSize : _totalPhysicalSize;

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString() {
            return base.ToString() + ", total physical size " + TotalPhysicalSize + " [" + Nested + "]";
        }

        [ContractInvariantMethod]
        private void ObjectInvariant() {
            Contract.Invariant(Nested != null);
            Contract.Invariant(TotalPhysicalSize >= 0);
        }
    }
}
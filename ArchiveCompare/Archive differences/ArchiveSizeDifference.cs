using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an archive difference by unpacked size. </summary>
    [DataContract(Name = "archiveSizeDiff", IsReference = true, Namespace = "")]
    public class ArchiveSizeDifference : ArchiveTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="ArchiveSizeDifference"/> class. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        public ArchiveSizeDifference([CanBeNull] Archive left, [CanBeNull] Archive right)
            : base(left, right) {
        }

        /// <summary> Gets the left unpacked size. </summary>
        [DataMember(Name = "lSize", Order = 0)]
        public long LeftSize { get; private set; }

        /// <summary> Gets the right unpacked size. </summary>
        [DataMember(Name = "rSize", Order = 1)]
        public long RightSize { get; private set; }

        /// <summary> Gets a value indicating whether the archives differ by this trait. </summary>
        public override bool DifferenceExists => LeftSize != RightSize;

        /// <summary> Returns a <see cref="System.String" /> that represents this instance. </summary>
        /// <returns> A <see cref="System.String" /> that represents this instance. </returns>
        public override string ToString() {
            return base.ToString() + $" ({LeftSize} v {RightSize})";
        }

        /// <summary> Initializes comparison from two single archives. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        /// <returns>true if comparison of two single archives by this trait can be performed;
        ///  false otherwise.</returns>
        protected override bool InitFromSingles(SingleArchive left, SingleArchive right) {
            LeftSize = left.Size;
            RightSize = right.Size;
            return true;
        }
    }
}
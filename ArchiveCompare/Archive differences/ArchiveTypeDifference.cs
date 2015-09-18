using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an archive difference by type. </summary>
    [DataContract(Name = "archiveTypeDiff", IsReference = true, Namespace = "")]
    public class ArchiveTypeDifference : ArchiveTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="ArchiveTypeDifference"/> class. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        public ArchiveTypeDifference([CanBeNull] Archive left, [CanBeNull] Archive right)
            : base(left, right) {
        }

        /// <summary> Gets the left archive type. </summary>
        [DataMember(Name = "lType", Order = 0)]
        public ArchiveType LeftType { get; private set; }

        /// <summary> Gets the right archive type. </summary>
        [DataMember(Name = "rType", Order = 1)]
        public ArchiveType RightType { get; private set; }

        /// <summary> Gets a value indicating whether the archives differ by this trait. </summary>
        public override bool DifferenceExists => LeftType != RightType;

        /// <summary> Returns a <see cref="System.String" /> that represents this instance. </summary>
        /// <returns> A <see cref="System.String" /> that represents this instance. </returns>
        public override string ToString() {
            return base.ToString() + $" ({Archive.TypeToString(LeftType)} v {Archive.TypeToString(RightType)})";
        }

        /// <summary> Initializes comparison from any two archives. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        /// <returns>true if comparison of any two archives by this trait can be performed; false otherwise.</returns>
        protected override bool InitFromArchives(Archive left, Archive right) {
            LeftType = left.Type;
            RightType = right.Type;
            return true;
        }
    }
}
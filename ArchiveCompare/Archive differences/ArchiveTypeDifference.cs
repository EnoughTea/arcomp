using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an archive difference by type. </summary>
    public class ArchiveTypeDifference : ArchiveTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="ArchiveTypeDifference"/> class. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        public ArchiveTypeDifference([CanBeNull] Archive left, [CanBeNull] Archive right)
            : base(left, right) {
        }

        /// <summary> Gets the left archive type. </summary>
        public ArchiveType LeftType { get; private set; }

        /// <summary> Gets the right archive type. </summary>
        public ArchiveType RightType { get; private set; }

        /// <summary> Gets a value indicating whether the archives differ by this trait. </summary>
        public override bool DifferenceExists => LeftType != RightType;

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
﻿using System.Diagnostics;
using JetBrains.Annotations;

namespace ArchiveCompare {
    /// <summary> Represents an archive difference by some trait. </summary>
    public abstract class ArchiveTraitDifference {
        /// <summary> Initializes a new instance of the <see cref="ArchiveTraitDifference"/> class. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        protected ArchiveTraitDifference([CanBeNull] Archive left, [CanBeNull] Archive right) {
            ComparisonExists = From(left, right);
        }

        /// <summary> Gets a value indicating whether it was possible to compare archives by this trait. </summary>
        public bool ComparisonExists { get; }

        /// <summary> Gets a value indicating whether the archives differ by this trait. </summary>
        public abstract bool DifferenceExists { get; }

        /// <summary> Initializes comparison from two single archives. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        /// <returns>true if comparison of two single archives by this trait can be performed;
        ///  false otherwise.</returns>
        protected virtual bool InitFromSingles(SingleArchive left, SingleArchive right) {
            return false;
        }

        /// <summary> Initializes comparison from two split archives. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        /// <returns>true if comparison of two split archives by this trait can be performed;
        ///  false otherwise.</returns>
        protected virtual bool InitFromSplits(SplitArchive left, SplitArchive right) {
            return InitFromSingles(left.Nested, right.Nested);
        }

        /// <summary> Initializes comparison from single and split archives. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        /// <returns>true if comparison of single and split archives by this trait can be performed;
        ///  false otherwise.</returns>
        protected virtual bool InitFromSingleAndSplit(SingleArchive left, SplitArchive right) {
            return InitFromSingles(left, right.Nested);
        }

        /// <summary> Initializes comparison from split and single archives. </summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        /// <returns>true if comparison of split and single archives by this trait can be performed;
        ///  false otherwise.</returns>
        protected virtual bool InitFromSplitAndSingle(SplitArchive left, SingleArchive right) {
            return InitFromSingles(left.Nested, right);
        }

        /// <summary> Compares two archives using appropriate difference-finding methods defined by
        ///  this comparison.</summary>
        /// <param name="left">Left archive.</param>
        /// <param name="right">Right archive.</param>
        /// <returns>true if comparison was possible; false if no suitable method was found for
        ///  passed archive type combination.</returns>
        private bool From([CanBeNull] Archive left, [CanBeNull] Archive right) {
            if (left == null || right == null) { return false; }

            var leftSplit = left as SplitArchive;
            var rightSplit = right as SplitArchive;
            var leftSingle = left as SingleArchive;
            var rightSingle = right as SingleArchive;

            bool comparedSuccesfully;
            try {
                if (leftSplit != null) {
                    if (rightSplit != null) {
                        comparedSuccesfully = InitFromSplits(leftSplit, rightSplit);
                    } else {
                        Debug.Assert(rightSingle != null);
                        comparedSuccesfully = InitFromSplitAndSingle(leftSplit, rightSingle);
                    }
                } else {
                    if (rightSplit != null) {
                        Debug.Assert(leftSingle != null);
                        comparedSuccesfully = InitFromSingleAndSplit(leftSingle, rightSplit);
                    } else {
                        Debug.Assert(leftSingle != null && rightSingle != null);
                        comparedSuccesfully = InitFromSingles(leftSingle, rightSingle);
                    }
                }
            } catch {
                comparedSuccesfully = false;
            }

            return comparedSuccesfully;
        }
    }
}

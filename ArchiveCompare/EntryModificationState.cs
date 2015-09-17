namespace ArchiveCompare {
    /// <summary> Possible entry state change between 'left' and 'right' versions. </summary>
    public enum EntryModificationState {
        /// <summary> Undefined state. </summary>
        Unknown,

        /// <summary> No change has been made. </summary>
        Same,

        /// <summary> Entry was non-existant in 'left' version, but was added in 'right' version. </summary>
        Added,

        /// <summary> Entry properties were modified between versions. </summary>
        Modified,

        /// <summary> Entry existed in 'left' version, but was removed in 'right' version. </summary>
        Removed
    }
}
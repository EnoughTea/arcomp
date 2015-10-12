namespace ArchiveCompare {
    /// <summary> Known archive types. </summary>
    public enum ArchiveType {
        /// <summary> Unknown archive type. </summary>
        Unknown,
        /// <summary> Split archive, consisting of several volumes. </summary>
        Split,
        /// <summary> Bethesda archive. </summary>
        Bsa,
        /// <summary> bzip2. </summary>
        BZip2,
        /// <summary> gzip. </summary>
        GZip,
        /// <summary> mbr. </summary>
        Mbr,
        /// <summary> PE. </summary>
        Pe,
        /// <summary> Rar. </summary>
        Rar,
        /// <summary> 7z. </summary>
        SevenZip,
        /// <summary> tar. </summary>
        Tar,
        /// <summary> xz. </summary>
        Xz,
        /// <summary> vhd. </summary>
        Vhd,
        /// <summary> zip. </summary>
        Zip
    }
}
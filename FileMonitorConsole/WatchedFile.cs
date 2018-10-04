using System;
using System.IO;

namespace FileMonitorConsole
{
    /// <summary>
    /// Represents a file and its attributes
    /// </summary>
    public class WatchedFile
    {
        /// <summary>
        /// The file this <see cref="WatchedFile"/> wraps
        /// </summary>
        public FileInfo File { get; set; }

        /// <summary>
        /// The watched directory that this file is in
        /// </summary>
        public DirectoryInfo RootDirectory { get; set; }

        /// <summary>
        /// Path starting from <see cref="RootDirectory"/>
        /// </summary>
        public string ShortPath
        {
            get
            {
                var root = new Uri(RootDirectory.FullName);
                return root.MakeRelativeUri(new Uri(File.FullName))
                    .OriginalString
                    .Replace('/', '\\')
                    .Replace("%20", " ");
            }
        }

        /// <summary>
        /// Size of the file in bytes
        /// </summary>
        public long ByteCount { get; set; }

        /// <summary>
        /// Value of the Archive bit
        /// </summary>
        public bool Archive { get; set; }

        /// <summary>
        /// Is this file hidden?
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Is this file read only?
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// Used to provide a uniform way of displaying date and time
        /// </summary>
        public const string DateFormatString = "M/d/yy (h:mm tt)";

        /// <summary>
        /// Date of the file was last opened
        /// </summary>
        public DateTime LastOpened { get; set; }

        /// <summary>
        /// Date the file was last written to
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Should we hash this file's contents?
        /// </summary>
        public bool ComputeHash { get; set; }

        /// <summary>
        /// SHA256 hash of this file's contents
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Creates an instance of the <see cref="WatchedFile"/> class
        /// </summary>
        /// <param name="file">The file this represents</param>
        /// <param name="rootDirectory">Watched directory this file is within</param>
        public WatchedFile(FileInfo file, DirectoryInfo rootDirectory)
        {
            File = file;
            RootDirectory = rootDirectory;
        }

        /// <summary>
        /// Set the properties of the <see cref="WatchedFile"/> to the updated properties of <see cref="File"/>
        /// </summary>
        public void Refresh()
        {
            File.Refresh();

            ByteCount = File.Length;

            Archive = File.Attributes.HasFlag(FileAttributes.Archive);
            Hidden = File.Attributes.HasFlag(FileAttributes.Hidden);
            ReadOnly = File.IsReadOnly;

            LastOpened = File.LastAccessTime;
            LastModified = File.LastWriteTime;

            if (ComputeHash)
            {
                try
                {
                    Hash = Crypto.Hash(System.IO.File.ReadAllText(File.FullName));
                }
                catch
                {
                    Global.Error("Could not read file: '" + File.FullName + "'.", exit: false);
                }
            }
        }

        /// <summary>
        /// Lists all of the <see cref="WatchedFile"/>'s properties
        /// </summary>
        public override string ToString()
        {
            return ShortPath + Environment.NewLine +
                "  Size:          " + ByteCount.ToString("n0") + " bytes" + Environment.NewLine +
                "  Archive:       " + Archive + Environment.NewLine +
                "  Hidden:        " + Hidden + Environment.NewLine +
                "  Readonly:      " + ReadOnly + Environment.NewLine +
                "  Last Opened:   " + LastOpened.ToString(DateFormatString) + Environment.NewLine +
                "  Last Modified: " + LastModified.ToString(DateFormatString);
        }
    }
}

using System;

namespace FileMonitorConsole
{
    /// <summary>
    /// Represents a change in a file
    /// </summary>
    public class Change
    {
        /// <summary>
        /// File affected by the change
        /// </summary>
        public WatchedFile File { get; private set; }

        /// <summary>
        /// Type of change that occured
        /// </summary>
        public ChangeType ChangeType { get; private set; }

        /// <summary>
        /// String representation of the change that occured
        /// </summary>
        public string Details { get; private set; }

        /// <summary>
        /// Creates an instance of the <see cref="Change"/> class
        /// </summary>
        /// <param name="file">File affected by the change</param>
        /// <param name="changeType">Type of change that occured</param>
        public Change(WatchedFile file, ChangeType changeType)
        {
            File = file;
            ChangeType = changeType;
            
            if (ChangeType == ChangeType.Added)
            {
                Details = "'" + File.ShortPath + "' was added.";
            }
            else if (ChangeType == ChangeType.Edited)
            {
                Details = "'" + File.ShortPath + "' was edited on " +
                    File.LastModified.ToString(WatchedFile.DateFormatString) + ".";
            }
            else if (ChangeType == ChangeType.Removed)
            {
                Details = "'" + File.ShortPath + "' was removed.";
            }
            else if (ChangeType == ChangeType.ArchiveChanged)
            {
                Details = "'" + File.ShortPath + "' archive changed to " + file.Archive.ToString() + ".";
            }
            else if (ChangeType == ChangeType.HiddenChanged)
            {
                Details = "'" + File.ShortPath + "' hidden changed to " + file.Hidden.ToString() + ".";
            }
            else if (ChangeType == ChangeType.ReadOnlyChanged)
            {
                Details = "'" + File.ShortPath + "' read only changed to " + file.ReadOnly.ToString() + ".";
            }
            else
            {
                throw new ArgumentException("Unsupported ChangeType", "changeType");
            }
        }

        /// <summary>
        /// Returns <see cref="Details"/>
        /// </summary>
        public override string ToString()
        {
            return Details;
        }
    }
}

using System.Collections.Generic;

namespace FileMonitorConsole
{
    /// <summary>
    /// IEqaulityComparer for <see cref="WatchedFile"/> objects,
    /// compares each object by their File's FullName (full path)
    /// </summary>
    public class WatchedFileComparer : IEqualityComparer<WatchedFile>
    {
        /// <summary>
        /// Compares two <see cref="WatchedFile"/> objects a and b,
        /// returning true both share the same path
        /// </summary>
        public bool Equals(WatchedFile a, WatchedFile b)
        {
            return a.File.FullName == b.File.FullName;
        }

        /// <summary>
        /// Returns a unique hash code based on the file's path
        /// </summary>
        public int GetHashCode(WatchedFile file)
        {
            return file.File.FullName.GetHashCode();
        }
    }
}

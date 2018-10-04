using System.Collections.Generic;

namespace FileMonitorConsole
{
    public class WatchedFileComparer : IEqualityComparer<WatchedFile>
    {
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

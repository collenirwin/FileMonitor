using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileMonitorConsole
{
    /// <summary>
    /// Represents a directory of <see cref="WatchedFile"/>s
    /// </summary>
    public class WatchedDirectory
    {
        /// <summary>
        /// The directory this <see cref="WatchedDirectory"/> wraps
        /// </summary>
        public DirectoryInfo RootDirectory { get; private set; }

        /// <summary>
        /// All files within <see cref="RootDirectory"/> represented as <see cref="WatchedFile"/>s
        /// </summary>
        public List<WatchedFile> Files { get; private set; }

        /// <summary>
        /// Should we hash each file's contents?
        /// </summary>
        public bool ComputeFileHashes { get; private set; }

        /// <summary>
        /// Creates an instance of the <see cref="WatchedDirectory"/> class
        /// </summary>
        /// <param name="rootDirectory">The directory this <see cref="WatchedDirectory"/> wraps</param>
        /// <param name="computeFileHashes">Should we hash each file's contents?</param>
        public WatchedDirectory(DirectoryInfo rootDirectory, bool computeFileHashes)
        {
            RootDirectory = rootDirectory;
            Files = new List<WatchedFile>();
            ComputeFileHashes = computeFileHashes;
        }

        /// <summary>
        /// Recursively searches for all files within <see cref="RootDirectory"/> and its subdirectories,
        /// and adds them to <see cref="Files"/>.
        /// </summary>
        /// <param name="writeToConsole">When set to true, each file will be echoed to the Console.</param>
        public void PopulateFiles(bool writeToConsole)
        {
            foreach (var file in RootDirectory.GetFiles("*", SearchOption.AllDirectories))
            {
                var watchedFile = new WatchedFile(file, RootDirectory);
                watchedFile.Refresh();

                Files.Add(watchedFile);

                if (writeToConsole)
                {
                    Console.WriteLine("Registered " + file.Name + ".");
                }
            }
        }

        /// <summary>
        /// Finds changes between this (old) <see cref="WatchedDirectory"/>'s <see cref="Files"/>
        /// and the given new copy of the <see cref="WatchedDirectory"/>'s <see cref="Files"/>
        /// </summary>
        /// <param name="newDirectory">Newer copy of this <see cref="WatchedDirectory"/></param>
        /// <returns>
        /// a list of <see cref="Change"/>s that reflect the differences
        /// between the two <see cref="WatchedDirectory"/>s' <see cref="Files"/>
        /// </returns>
        public List<Change> FindChanges(WatchedDirectory newDirectory)
        {
            var changes = new List<Change>();

            var comparer = new WatchedFileComparer();

            // find all files that were added since the watch, add them to the changes
            var addedFiles = newDirectory.Files.Except(Files, comparer);

            foreach (var file in addedFiles)
            {
                changes.Add(new Change(file, ChangeType.Added));
            }

            // find all files that were removed since the watch, add them to the changes
            var removedFiles = Files.Except(newDirectory.Files, comparer);

            foreach (var file in removedFiles)
            {
                changes.Add(new Change(file, ChangeType.Removed));
            }

            // get all the files that both directories share
            var commonFilesNew = newDirectory.Files.Intersect(Files, comparer).ToArray();
            var commonFilesOld = Files.Intersect(newDirectory.Files, comparer).ToArray();

            for (int x = 0; x < commonFilesNew.Length; x++)
            {
                var oldFile = commonFilesOld[x];
                var newFile = commonFilesNew[x];

                // file text changed
                if (ComputeFileHashes)
                {
                    if (newFile.Hash != oldFile.Hash)
                    {
                        changes.Add(new Change(newFile, ChangeType.Edited));
                    }
                }
                else
                {
                    if (newFile.LastModified != oldFile.LastModified)
                    {
                        changes.Add(new Change(newFile, ChangeType.Edited));
                    }
                }

                // file archive attribute changed
                if (newFile.Archive != oldFile.Archive)
                {
                    changes.Add(new Change(newFile, ChangeType.ArchiveChanged));
                }

                // file hidden attribute changed
                if (newFile.Hidden != oldFile.Hidden)
                {
                    changes.Add(new Change(newFile, ChangeType.HiddenChanged));
                }

                // file readonly attribute changed
                if (newFile.ReadOnly != oldFile.ReadOnly)
                {
                    changes.Add(new Change(newFile, ChangeType.ReadOnlyChanged));
                }
            }

            return changes;
        }
    }
}

using Newtonsoft.Json;
using RegExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileMonitorConsole
{
    /// <summary>
    /// Entry point class for the app
    /// </summary>
    class Program
    {
        /// <summary>
        /// Path to our subdirectory within the user's AppData directory
        /// </summary>
        private static readonly string appDataDirectoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FileMonitor");

        /// <summary>
        /// Dictionary of watched directory paths with the date they were watched
        /// </summary>
        private static Dictionary<string, DateTime> watchedDirectories;

        /// <summary>
        /// Path to our serialized <see cref="watchedDirectories"/> list
        /// </summary>
        private static readonly string watchedDirectoryFilePath =
            Path.Combine(appDataDirectoryPath, "watched_directories.json");

        #region Main

        /// <summary>
        /// Entry point method for the app
        /// </summary>
        /// <param name="args">Command line arguments</param>
        private static void Main(string[] args)
        {
            // make sure our AppData folder is there
            var appDataDirectory = new DirectoryInfo(appDataDirectoryPath);
 
            if (!appDataDirectory.Exists)
            {
                appDataDirectory.Create();
            }

            // load our list of watched directories in
            watchedDirectories = loadWatchedDirectoryList();

            // find watched directories whose files have been removed from our AppData folder
            var removedDirectories = watchedDirectories
                .Keys
                .Where(x => !File.Exists(getHashedFilePath(x)))
                .ToArray();

            // remove them from the dictionary
            for (int x = removedDirectories.Length - 1; x > -1; x--)
            {
                watchedDirectories.Remove(removedDirectories[x]);
            }

            bool badArgs = false;

            // clean up args
            if (args != null)
            {
                // make everything lowercase
                args = args.Select(x => x.ToLower()).ToArray();
            }

            // check args for commands

            // default to the help command
            if (args == null || args.Length == 0)
            {
                help();
            }
            else if (args.Length == 1)
            {
                string arg = args[0];

                if (arg == "help")
                {
                    help();
                }
                else if (arg == "list")
                {
                    list();
                }
                else
                {
                    badArgs = true;
                }
            }
            else if (args.Length < 2)
            {
                badArgs = true;
            }
            else if (args[0] == "watch")
            {
                watch(args[1], computeHash: false);
            }
            else if (args[0] == "watch-hash")
            {
                watch(args[1], computeHash: true);
            }
            else if (args[0] == "check")
            {
                check(args[1]);
            }
            else if (args[0] == "state")
            {
                if (args.Length == 2)
                {
                    state(args[1]);
                }
                else if (args.Length == 3)
                {
                    state(args[1], args[2]);
                }
                else
                {
                    badArgs = true;
                }
            }
            else
            {
                badArgs = true;
            }

            if (badArgs)
            {
                Global.Error("Invalid command. Use the help command to see a list of valid commands.");
            }

            // serialize and save watchedDirectories before exiting
            string json = JsonConvert.SerializeObject(watchedDirectories);
            File.WriteAllText(watchedDirectoryFilePath, json);
        }

        #endregion

        #region Commands

        /// <summary>
        /// List all available commands and their descriptions
        /// </summary>
        private static void help()
        {
            Console.WriteLine("FileMonitorConsole Help");
            Console.WriteLine("help - List commands");
            Console.WriteLine("watch <directory> - Register a directory for monitoring");
            Console.WriteLine("watch-hash <directory> - Register a directory for monitoring, keeping a hash" +
                Environment.NewLine + "  of each file's contents for precision (slow when dealing with large files)");
            Console.WriteLine("check <watched_directory> - Check if the watched directory has been changed");
            Console.WriteLine("state <watched_directory> - List all watched files with their saved properties");
            Console.WriteLine("state <watched_directory> <regex_pattern> - List all watched files whose names" +
                Environment.NewLine + "  match the specified regular expression pattern with their saved properties");
            Console.WriteLine("list - List all watched directories");
        }

        /// <summary>
        /// Register a directory for monitoring
        /// </summary>
        /// <param name="path">Path to directory</param>
        /// <param name="computeHash">Should we hash each watched file's contents?</param>
        private static void watch(string path, bool computeHash)
        {
            var directory = getDirectory(path);

            var watchedDirectory = new WatchedDirectory(directory, computeHash);

            // grab all files within the directory
            watchedDirectory.PopulateFiles(writeToConsole: true);

            Console.WriteLine("Serializing " + directory.FullName);
            string json = JsonConvert.SerializeObject(watchedDirectory);

            // save as a hash of the full path
            string filePath = getHashedFilePath(directory.FullName);

            Console.WriteLine("Writing to " + filePath);
            File.WriteAllText(filePath, json);

            // update our dictionay entry for this directory or make a new one
            if (watchedDirectories.ContainsKey(directory.FullName))
            {
                watchedDirectories[directory.FullName] = DateTime.Now;
            }
            else
            {
                watchedDirectories.Add(directory.FullName, DateTime.Now);
            }

            Console.WriteLine("Done.");
        }

        /// <summary>
        /// Check for changes in a registered directory
        /// </summary>
        /// <param name="path">Path to watched directory</param>
        private static void check(string path)
        {
            var directory = getDirectory(path);

            var oldDirectory = getWatchedDirectoryFromFile(directory.FullName);

            // register the directory structure again
            var newDirectory = new WatchedDirectory(directory, oldDirectory.ComputeFileHashes);
            newDirectory.PopulateFiles(writeToConsole: false);

            var changes = oldDirectory.FindChanges(newDirectory);

            foreach (var change in changes)
            {
                Console.WriteLine(change.ToString());
            }

            if (!changes.Any())
            {
                Console.WriteLine("No changes were found.");
            }

            Console.WriteLine("Do you want to re-watch this directory? [y/n]");

            if (Console.ReadLine().ToLower() == "y")
            {
                watch(directory.FullName, oldDirectory.ComputeFileHashes);
            }
        }

        /// <summary>
        /// List all watched files in the watched directory with all of their attributes
        /// </summary>
        /// <param name="path">Path to watched directory</param>
        /// <param name="pattern">
        /// Regular Expression pattern the watched files must match in order to be displayed
        /// </param>
        private static void state(string path, string pattern = null)
        {
            var directory = getDirectory(path);

            var watchedDirectory = getWatchedDirectoryFromFile(directory.FullName);

            if (pattern == null)
            {
                foreach (var file in watchedDirectory.Files)
                {
                    Console.WriteLine(file.ToString());
                }
            }
            else
            {
                foreach (var file in watchedDirectory.Files)
                {
                    if (file.File.Name.IsMatch(pattern))
                    {
                        Console.WriteLine(file.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// List all watched directories
        /// </summary>
        private static void list()
        {
            foreach (string path in watchedDirectories.Keys)
            {
                Console.WriteLine(path + " watched on " +
                    watchedDirectories[path].ToString(WatchedFile.DateFormatString));
            }

            if (!watchedDirectories.Any())
            {
                Console.WriteLine("You have no watched directories.");
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Deserializes watched_directories.json within <see cref="appDataDirectoryPath"/>
        /// </summary>
        /// <returns>The deserialized list or an empty list</returns>
        private static Dictionary<string, DateTime> loadWatchedDirectoryList()
        {
            try
            {
                string json = File.ReadAllText(watchedDirectoryFilePath);
                return JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(json)
                    ?? new Dictionary<string, DateTime>();
            }
            catch
            {
                return new Dictionary<string, DateTime>();
            }
        }

        /// <summary>
        /// Deserialize a <see cref="WatchedDirectory"/> from a given file and return it
        /// </summary>
        /// <param name="path">Path to the json file</param>
        /// <returns>The deserialized <see cref="WatchedDirectory"/> object</returns>
        private static WatchedDirectory getWatchedDirectoryFromFile(string path)
        {
            var jsonFile = new FileInfo(getHashedFilePath(path));

            if (!jsonFile.Exists)
            {
                Global.Error("Directory '" + path + "' is not watched.");
            }

            // read and deserialize the json file
            return JsonConvert.DeserializeObject<WatchedDirectory>(File.ReadAllText(jsonFile.FullName));
        }

        /// <summary>
        /// Hashes the given directory path and combines it with <see cref="appDataDirectoryPath"/>,
        /// then the .json file extension is appended
        /// </summary>
        /// <param name="path">Path to the watched directory</param>
        /// <returns>Appropriate path to use for writing the json file</returns>
        private static string getHashedFilePath(string path)
        {
            return Path.Combine(appDataDirectoryPath, Crypto.Hash(path) + ".json");
        }

        /// <summary>
        /// Get the directory from a local or full path
        /// </summary>
        /// <param name="path">Path to directory</param>
        private static DirectoryInfo getDirectory(string path)
        {
            var directory = new DirectoryInfo(Path.GetFullPath(path));

            if (!directory.Exists)
            {
                Global.Error("Directory '" + directory.FullName + "' does not exist.");
            }

            return directory;
        }

        #endregion
    }
}

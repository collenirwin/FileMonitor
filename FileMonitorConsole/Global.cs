using System;

namespace FileMonitorConsole
{
    /// <summary>
    /// Provides static fields and methods for use throughout the app
    /// </summary>
    public static class Global
    {
        /// <summary>
        /// The user's default console color
        /// </summary>
        public static readonly ConsoleColor DefaultColor = Console.ForegroundColor;

        /// <summary>
        /// Writes the specified 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exit">Exit the app?</param>
        public static void Error(string message, bool exit = true)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine(message);

            Console.ForegroundColor = DefaultColor;

            if (exit)
            {
                Environment.Exit(1);
            }
        }
    }
}

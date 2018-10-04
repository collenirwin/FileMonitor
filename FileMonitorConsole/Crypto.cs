using System.Security.Cryptography;
using System.Text;

namespace FileMonitorConsole
{
    /// <summary>
    /// Provides hashing methods
    /// </summary>
    public static class Crypto
    {
        /// <summary>
        /// Returns a sha256 hash of the given string in base 16
        /// </summary>
        public static string Hash(string text)
        {
            var hashString = new StringBuilder();

            // sha256 hash
            using (var sha256 = new SHA256Managed())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));

                foreach (byte b in bytes)
                {
                    // append bytes in base 16
                    hashString.Append(b.ToString("x2"));
                }
            }

            return hashString.ToString();
        }
    }
}

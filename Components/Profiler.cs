using System;
using System.IO;

namespace Opeity.Components {
    /// <summary>
    /// User Profile Controller Class
    /// </summary>
    public class Profiler {
        /// <summary>
        /// If the supplied direcrory doesn't exist create it
        /// </summary>
        /// <param name="_Path">Path to Directory to check</param>
        public static void CheckCreateDirectory(String _Path) {
            if (Directory.Exists(_Path))
                return;

            Directory.CreateDirectory(_Path);
        }

        /// <summary>
        /// Gets the username of the currently logged in Windows user
        /// Returns "_Default" if it can't get the username
        /// </summary>
        /// <returns>Windows Username</returns>
        public static String GetUserName() {
            String[] Identity = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\');

            if (Identity.Length >= 1) {
                return Identity[1];
            }

            return "_Default";
        }

        /// <summary>
        /// Base Profile directory containing all Profile Directories
        /// </summary>
        public static String BaseDir {
            get {
                String _Path = Path.Combine(Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory), "Profiles");
                CheckCreateDirectory(_Path);
                return _Path;
            }
        }

        /// <summary>
        /// User Profile directory used for storing user specific items
        /// </summary>
        public static String UserProfileBase {
            get {
                String _Path = Path.Combine(BaseDir, GetUserName());
                CheckCreateDirectory(_Path);
                return _Path;
            }
        }
        
        /// <summary>
        /// Path to the currently logged in users Favorites database file
        /// </summary>
        public static String UserFavorites {
            get {
                return Path.Combine(
                    Profiler.UserProfileBase,
                    "Favorites"
                );
            }
        }

        /// <summary>
        /// Path to the currently logged in users Preferences database file
        /// </summary>
        public static String UserPreferences {
            get {
                return Path.Combine(
                    Profiler.UserProfileBase,
                    "Preferences"
                );
            }
        }
    }
}

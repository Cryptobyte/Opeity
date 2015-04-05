using System;

namespace Opeity.Components.Classes {
    /// <summary>
    /// User Preferences class
    /// </summary>
    public class Preferences {
        /// <summary>
        /// Browser default home page
        /// </summary>
        public String Home { get; set; }

        /// <summary>
        /// Browser user agent
        /// </summary>
        public String UserAgent { get; set; }

        /// <summary>
        /// Last Url Visited
        /// </summary>
        public String LastUrl { get; set; }

        /// <summary>
        /// Uses Awesomium's function to free process memory on browser navigation
        /// </summary>
        public bool FreeMemoryOnNav { get; set; }

        /// <summary>
        /// Forces the browser to maintain a single window
        /// </summary>
        public bool ForceSingleWindow { get; set; }

        /// <summary>
        /// Allows or disallows web scripts access to clipboard contents
        /// </summary>
        public bool CanScriptsAccessClipboard { get; set; }

        /// <summary>
        /// Allows or disallows HTML5 databases
        /// </summary>
        public bool HTML5Databases { get; set; }

        /// <summary>
        /// Enables or disables browser GPU acceleration
        /// </summary>
        public bool EnableGPUAcceleration { get; set; }

        /// <summary>
        /// Enables or disables WebGL
        /// </summary>
        public bool EnableWebGL { get; set; }

        /// <summary>
        /// Enables or disables restoring the last visited page on browser launch
        /// </summary>
        public bool SessionRestore { get; set; }

        /// <summary>
        /// Forces WPF components to render at specific FPS
        /// </summary>
        public int FPSLock { get; set; }

        /// <summary>
        /// Class main entry point, also defines default settings
        /// </summary>
        public Preferences() {
            Home = "http://www.google.com/";
            UserAgent = "Mozilla/5.0 (Windows NT 6.3) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.101 Safari/537.36";
            FreeMemoryOnNav = true;
            ForceSingleWindow = true;
            CanScriptsAccessClipboard = true;
            HTML5Databases = true;
            EnableGPUAcceleration = true;
            EnableWebGL = true;
            SessionRestore = false;
            FPSLock = 60;
        }
    }
}

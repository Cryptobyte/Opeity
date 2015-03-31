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
        /// Uses Awesomium's function to free process memory on browser navigation
        /// </summary>
        public bool FreeMemoryOnNav { get; set; }

        /// <summary>
        /// Forces the browser to maintain a single window
        /// </summary>
        public bool ForceSingleWindow { get; set; }


        /// <summary>
        /// Class main entry point
        /// </summary>
        public Preferences() {
            Home = "http://www.google.com/";
            UserAgent = "Mozilla/5.0 (Windows NT 6.3) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.101 Safari/537.36";
            FreeMemoryOnNav = true;
            ForceSingleWindow = true;
        }
    }
}

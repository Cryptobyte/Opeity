using System.IO;
using System.Windows;
using Awesomium.Core;
using Opeity.Properties;

namespace Opeity {
    public partial class App : Application {
        /// <summary>
        /// Occurs on application startup
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e) {
            /*
             * Opeity_Renderer.exe is our C++ Child Process (not included in Github codebase)
             * 
             * If you want to build without this, simply delete the ChildProcessPath line and 
             * Awesomium will use its default child process awesomium_process
             */
            if (!WebCore.IsInitialized) {
                WebCore.Initialize(new WebConfig() {
                    HomeURL = new System.Uri(Settings.Default.Home),
                    ReduceMemoryUsageOnNavigation = Settings.Default.FreeMemoryOnNav,
                    UserAgent = Settings.Default.UserAgent,
                    ChildProcessPath = Path.Combine(Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory), "Opeity_Renderer.exe")
                });
            }

            base.OnStartup(e);
        }

        /// <summary>
        /// Occurs on application exit
        /// </summary>
        /// <param name="e"></param>
        protected override void OnExit(ExitEventArgs e) {
            if (WebCore.IsInitialized)
                WebCore.Shutdown();

            base.OnExit(e);
        }
    }
}

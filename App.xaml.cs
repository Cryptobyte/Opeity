using System.IO;
using System.Windows;
using Awesomium.Core;
using Opeity.Components;
using Opeity.Properties;

namespace Opeity {
    public partial class App : Application {
        /// <summary>
        /// Occurs on application startup
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e) {
            PrefAdapter Prefs = new PrefAdapter();

            if (!WebCore.IsInitialized) {
                WebCore.Initialize(new WebConfig() {
                    HomeURL = new System.Uri(Prefs.UserPreferences.Home),
                    ReduceMemoryUsageOnNavigation = Prefs.UserPreferences.FreeMemoryOnNav,
                    UserAgent = Prefs.UserPreferences.UserAgent,
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

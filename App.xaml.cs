using System.Windows;
using Awesomium.Core;
using Opeity.Properties;

namespace Opeity {
    public partial class App : Application {
        protected override void OnStartup(StartupEventArgs e) {
            if (!WebCore.IsInitialized) {
                WebCore.Initialize(new WebConfig() {
                    HomeURL = new System.Uri(Settings.Default.Home),
                    ReduceMemoryUsageOnNavigation = true,
                    UserAgent = Settings.Default.UserAgent
                });
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e) {
            if (WebCore.IsInitialized)
                WebCore.Shutdown();

            base.OnExit(e);
        }
    }
}

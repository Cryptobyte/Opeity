using System;
using System.Windows;
using System.Windows.Media.Animation;
using Awesomium.Core;
using MahApps.Metro.Controls;
using Opeity.Components;
using Opeity.Properties;

namespace Opeity {
    public partial class MainWindow : MetroWindow {
        public Profiler Profile = new Profiler();
        WebSession Session;

        public MainWindow() {
            if (!WebCore.IsInitialized) {
                WebCore.Initialize(new WebConfig() {
                    ReduceMemoryUsageOnNavigation = true,
                    LogLevel = LogLevel.None
                });
            }

            InitializeComponent();

            Session = WebCore.Sessions[Profile.UserProfileBase] ?? WebCore.CreateWebSession(Profile.UserProfileBase, WebPreferences.Default);
            webControl.WebSession = Session;
            
            if (Environment.GetCommandLineArgs().Length > 1)
                webControl.Source = new Uri(Environment.GetCommandLineArgs()[1]);
            else
                webControl.Source = new Uri(Settings.Default.Home);
        }

        private void C_BTN_Back_Click(object sender, RoutedEventArgs e) {
            if (webControl.CanGoBack())
                webControl.GoBack();
        }

        private void C_BTN_Home_Click(object sender, RoutedEventArgs e) {
            webControl.Source = new Uri(Settings.Default.Home);
        }

        private void C_BTN_Refresh_Click(object sender, RoutedEventArgs e) {
            webControl.Reload(false);
        }

        private void C_BTN_Forward_Click(object sender, RoutedEventArgs e) {
            if (webControl.CanGoForward())
                webControl.GoForward();
        }

        private void C_BTN_Favorites_Click(object sender, RoutedEventArgs e) {
            var flyout = this.Flyouts.Items[0] as Flyout;
            flyout.IsOpen = !flyout.IsOpen;
        }

        private void C_BTN_Settings_Click(object sender, RoutedEventArgs e) {
            var flyout = this.Flyouts.Items[1] as Flyout;
            flyout.IsOpen = !flyout.IsOpen;
        }

        private void MetroWindow_Closed(object sender, EventArgs e) {
            if (Settings.Default.Windows <= 1) {
                webControl.Dispose();
                WebCore.Shutdown();
            }
        }

        private void webControl_LoadingFrame(object sender, LoadingFrameEventArgs e) {
            Storyboard board = new Storyboard();
            DoubleAnimation PR_S_In = new DoubleAnimation(0, 5, new Duration(TimeSpan.FromMilliseconds(200)));

            board.Children.Add(PR_S_In);
            Storyboard.SetTarget(PR_S_In, Prog);
            Storyboard.SetTargetProperty(PR_S_In, new PropertyPath("(Height)"));
            board.Begin();

            C_BTN_Refresh.Visibility = System.Windows.Visibility.Collapsed;
            C_BTN_Stop.Visibility = System.Windows.Visibility.Visible;
        }

        private void webControl_LoadingFrameComplete(object sender, FrameEventArgs e) {
            Storyboard board = new Storyboard();
            DoubleAnimation PR_S_Ou = new DoubleAnimation(5, 0, new Duration(TimeSpan.FromMilliseconds(200)));

            board.Children.Add(PR_S_Ou);
            Storyboard.SetTarget(PR_S_Ou, Prog);
            Storyboard.SetTargetProperty(PR_S_Ou, new PropertyPath("(Height)"));
            board.Begin();

            C_BTN_Stop.Visibility = System.Windows.Visibility.Collapsed;
            C_BTN_Refresh.Visibility = System.Windows.Visibility.Visible;
        }

        private void webControl_LoadingFrameFailed(object sender, LoadingFrameFailedEventArgs e) {
            C_BTN_Stop.Visibility = System.Windows.Visibility.Collapsed;
            C_BTN_Refresh.Visibility = System.Windows.Visibility.Visible;
        }

        private void webControl_Crashed(object sender, CrashedEventArgs e) {
            C_BTN_Stop.Visibility = System.Windows.Visibility.Collapsed;
            C_BTN_Refresh.Visibility = System.Windows.Visibility.Visible;
        }

        private void webControl_TitleChanged(object sender, TitleChangedEventArgs e) {
            this.Title = e.Title;
        }

        private void C_BTN_Refresh_iCache(object sender, RoutedEventArgs e) {
            webControl.Reload(true);
        }

        private void C_BTN_Stop_Click(object sender, RoutedEventArgs e) {
            webControl.Stop();
        }
    }
}

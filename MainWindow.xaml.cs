using System;
using System.Windows;
using System.Windows.Threading;
using Awesomium.Core;
using MahApps.Metro.Controls;
using NDatabase;
using Opeity.Components;
using Opeity.Components.Classes;

namespace Opeity {
    public partial class MainWindow : MetroWindow {
        WebSession Session;
        PrefAdapter Prefs;

        #region Helper Methods

        /// <summary>
        /// Checks if the WebCore has active downloads
        /// </summary>
        /// <returns>True if WebCore has active downloads, False if no active downloads</returns>
        private bool HasActiveDownloads() {
            WebCore.Downloads.ClearNonActive();
            return WebCore.Downloads.Count > 0;
        }

        /// <summary>
        /// Opens the Notify Flyout for a short time
        /// </summary>
        /// <param name="Message">Message to show user</param>
        private void Notify(String Message) {
            var flyout = this.Flyouts.Items[2] as Flyout;

            if (flyout.IsOpen)
                return;

            Notify_Label.Content = Message;

            DispatcherTimer dTx = new DispatcherTimer();

            dTx.Tick += delegate {
                dTx.Stop();

                if (flyout.IsOpen)
                    flyout.IsOpen = false;
            };

            dTx.Interval = new TimeSpan(0, 0, 3);
            dTx.Start();

            flyout.IsOpen = true;
        }

        /// <summary>
        /// Loads the Favorites list to the DataGrid
        /// This is probably temporary because in WPF binding is more efficient
        /// </summary>
        private void LoadFavorites() {
            if (GV_Favorites.Items.Count > 0)
                GV_Favorites.Items.Clear();

            using (var _odb = OdbFactory.Open(Profiler.UserFavorites)) {
                var _Favorites = _odb.QueryAndExecute<Favorite>();

                foreach (Favorite _Fav in _Favorites) {
                    GV_Favorites.Items.Add(_Fav);
                }
            }
        }

        /// <summary>
        /// Checks if a Favorite exists in the database using only a supplied Url
        /// </summary>
        /// <param name="Url">Url to check for in Favorites database</param>
        /// <returns></returns>
        private bool FavoriteExists(String Url) {
            using (var _odb = OdbFactory.Open(Profiler.UserFavorites)) {
                var _Favorites = _odb.QueryAndExecute<Favorite>();

                foreach (Favorite _Fav in _Favorites) {
                    if (_Fav.Url == Url)
                        return true;
                }
            }

            return false;
        }

        #endregion

        /// <summary>
        /// Main entry point
        /// </summary>
        public MainWindow() {
            InitializeComponent();

            #region Session Logic

            Session = WebCore.Sessions[Profiler.UserProfileBase] ?? WebCore.CreateWebSession(Profiler.UserProfileBase, WebPreferences.Default);
            webControl.WebSession = Session;

            Prefs = new PrefAdapter();

            #endregion

            #region Web Preferences

            WebPreferences.Default.CanScriptsAccessClipboard = true;
            WebPreferences.Default.Databases = true;
            WebPreferences.Default.EnableGPUAcceleration = true;
            WebPreferences.Default.FileAccessFromFileURL = true;
            WebPreferences.Default.ProxyConfig = "auto";
            WebPreferences.Default.UniversalAccessFromFileURL = true;
            WebPreferences.Default.WebGL = true;
            WebCore.DownloadBegin += OnDownloadBegin;

            #endregion

            LoadFavorites();
        }

        private void OnDownloadBegin(object sender, DownloadBeginEventArgs e) {
            Notify(String.Format("Downloading {0}", e.Info.FileName));

            e.Info.Completed += delegate {
                if (!HasActiveDownloads()) {
                    //All Downloads Finished
                }
            };
        }

        #region Window Browser Controls

        private void C_BTN_Back_Click(object sender, RoutedEventArgs e) {
            if (webControl.CanGoBack())
                webControl.GoBack();
        }

        private void C_BTN_Home_Click(object sender, RoutedEventArgs e) {
            webControl.GoToHome();
        }

        private void C_BTN_Refresh_Click(object sender, RoutedEventArgs e) {
            if (webControl.IsNavigating)
                webControl.Stop();
            else
                webControl.Reload(false);
        }

        private void C_BTN_Forward_Click(object sender, RoutedEventArgs e) {
            if (webControl.CanGoForward())
                webControl.GoForward();
        }

        #endregion

        #region Favorites Flyout

        private void C_BTN_Favorites_Click(object sender, RoutedEventArgs e) {
            TBox_Title.Text = webControl.Title;
            TBox_Url.Text = webControl.Source.ToString();

            var flyout = this.Flyouts.Items[0] as Flyout;
            flyout.IsOpen = !flyout.IsOpen;
        }

        private void Btn_Fav_Add_Click(object sender, RoutedEventArgs e) {
            var flyout = this.Flyouts.Items[0] as Flyout;
            if (flyout.IsOpen)
                flyout.IsOpen = false;

            Favorite _toAdd = new Favorite() {
                Title = TBox_Title.Text,
                Url = TBox_Url.Text
            };

            using (var _odb = OdbFactory.Open(Profiler.UserFavorites)) {
                _odb.Store(_toAdd);
            }

            LoadFavorites();
        }

        private void Btn_Fav_Cancel_Click(object sender, RoutedEventArgs e) {
            var flyout = this.Flyouts.Items[0] as Flyout;
            if (flyout.IsOpen)
                flyout.IsOpen = false;
        }

        private void GV_Favorites_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var flyout = this.Flyouts.Items[0] as Flyout;
            if (flyout.IsOpen)
                flyout.IsOpen = false;

            webControl.Source = new Uri(((Favorite)GV_Favorites.SelectedItem).Url);

            if (GV_Favorites.SelectedItem != null)
                GV_Favorites.SelectedItem = null;
        }

        #endregion

        #region Settings Flyout

        private void C_BTN_Settings_Click(object sender, RoutedEventArgs e) {
            var flyout = this.Flyouts.Items[1] as Flyout;
            flyout.IsOpen = !flyout.IsOpen;
        }

        #endregion

        #region Frame Events

        private void webControl_LoadingFrame(object sender, LoadingFrameEventArgs e) {
            
        }

        private void webControl_LoadingFrameComplete(object sender, FrameEventArgs e) {
            if (FavoriteExists(webControl.Source.ToString())) {
                C_BTN_Favorites.Visibility = System.Windows.Visibility.Collapsed;
                C_BTN_Favorites_H.Visibility = System.Windows.Visibility.Visible;
            } else {
                C_BTN_Favorites_H.Visibility = System.Windows.Visibility.Collapsed;
                C_BTN_Favorites.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void webControl_DocumentReady(object sender, DocumentReadyEventArgs e) {

        }

        private void webControl_LoadingFrameFailed(object sender, LoadingFrameFailedEventArgs e) {
            
        }

        private void webControl_Crashed(object sender, CrashedEventArgs e) {
            
        }

        #endregion

        private void C_BTN_Refresh_iCache(object sender, RoutedEventArgs e) {
            webControl.Reload(true);
        }

        private void C_BTN_Stop_Click(object sender, RoutedEventArgs e) {
            webControl.Stop();
        }

        private void webControl_JavascriptRequest(object sender, JavascriptRequestEventArgs e) {
            if (!e.RequestToken)
                return;

            if (e.Request != JavascriptRequest.Minimize) {

                if (this.WindowState != System.Windows.WindowState.Minimized)
                    this.WindowState = System.Windows.WindowState.Minimized;

                e.Handled = true;
            }

            if (e.Request != JavascriptRequest.Maximize) {

                if (this.WindowState != System.Windows.WindowState.Maximized)
                    this.WindowState = System.Windows.WindowState.Maximized;

                e.Handled = true;
            }

            if (e.Request != JavascriptRequest.Restore) {
                
                if ((this.WindowState == System.Windows.WindowState.Minimized) ||
                    (this.WindowState == System.Windows.WindowState.Maximized)) {
                        this.WindowState = System.Windows.WindowState.Normal;
                }

                e.Handled = true;
            }
        }

        private void webControl_ShowJavascriptDialog(object sender, JavascriptDialogEventArgs e) {
            
        }

        private void webControl_WindowClose(object sender, WindowCloseEventArgs e) {
            if (!e.IsCalledFromFrame)
                this.Close();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e) {
            if (Environment.GetCommandLineArgs().Length > 1)
                webControl.Source = new Uri(Environment.GetCommandLineArgs()[1]);
            else
                webControl.GoToHome();
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (HasActiveDownloads()) {
                //Downloads are still running!
            }

            Prefs.Save();
        }

        private void webControl_ShowCreatedWebView(object sender, ShowCreatedWebViewEventArgs e) {
            if (webControl == null)
                return;

            if (!webControl.IsLive)
                return;

            e.Cancel = true;

            webControl.Source = e.TargetURL; //Force all popups into the main view (Temporary)
        }
    }
}

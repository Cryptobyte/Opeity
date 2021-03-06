﻿using System;
using System.Windows;
using System.Windows.Media.Animation;
using Awesomium.Core;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NDatabase;
using Opeity.Components;
using Opeity.Components.Classes;
using Opeity.Properties;

namespace Opeity {
    public partial class MainWindow : MetroWindow {

        #region Globals

        WebSession Session;

        #endregion

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

        /// <summary>
        /// Converts cryptic file size to readable file size for display
        /// </summary>
        /// <param name="inSize">Cryptic file size to convert</param>
        /// <returns>Human readable file size</returns>
        private String ReadableSize(double inSize) {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;

            while (inSize >= 1024 && order + 1 < sizes.Length) {
                order++;
                inSize = inSize / 1024;
            }

            return String.Format("{0:0.#}{1}", inSize, sizes[order]);
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
            webControl.NavigationInfo = NavigationInfo.Normal;

            Timeline.DesiredFrameRateProperty.OverrideMetadata(
                typeof(Timeline),
                new FrameworkPropertyMetadata { DefaultValue = Settings.Default.FPSLock }
            );

            WebPreferences.Default.CanScriptsAccessClipboard = Settings.Default.CanScriptsAccessClipboard;
            WebPreferences.Default.Databases = Settings.Default.HTML5Databases;
            WebPreferences.Default.EnableGPUAcceleration = Settings.Default.EnableGPUAcceleration;
            WebPreferences.Default.WebGL = Settings.Default.EnableWebGL;
            WebPreferences.Default.UniversalAccessFromFileURL = true;
            WebPreferences.Default.FileAccessFromFileURL = true;
            WebPreferences.Default.ProxyConfig = "auto";

            #endregion

            WebCore.DownloadBegin += OnDownloadBegin;

            LoadFavorites();
        }

        #region File Downloads

        private async void OnDownloadBegin(object sender, DownloadBeginEventArgs e) {
            DateTime startTime = DateTime.Now;
            var _DController = await this.ShowProgressAsync("Downloading..", "Waiting..");

            _DController.SetCancelable(e.Info.CanCancel());

            e.Info.ProgressChanged += delegate {
                TimeSpan timeSpent = DateTime.Now - startTime;

                if (_DController.IsCanceled)
                    e.Info.Cancel();

                _DController.SetMessage(
                    String.Format(
                        "File: {0}{7}Source: {1}{7}Destination: {2}{7}{7}{3} of {4} at {5}/s (About {6} seconds remaining)",
                        e.Info.FileName,
                        e.Info.Url,
                        e.Info.SavePath,
                        ReadableSize(e.Info.ReceivedBytes), 
                        ReadableSize(e.Info.TotalBytes), 
                        ReadableSize(e.Info.CurrentSpeed),
                        (int)(timeSpent.TotalSeconds / e.Info.Progress * (100 - e.Info.Progress)),
                        Environment.NewLine
                    )
                 );

                try {
                    _DController.SetProgress((double)e.Info.Progress);
                } catch { _DController.SetIndeterminate(); }
            };

            e.Info.Canceled += delegate {
                _DController.CloseAsync();
            };

            e.Info.Completed += delegate {
                _DController.CloseAsync();
            };
        }

        #endregion

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

            if (FavoriteExists(webControl.Source.ToString())) {
                C_BTN_Favorites.Visibility = System.Windows.Visibility.Collapsed;
                C_BTN_Favorites_H.Visibility = System.Windows.Visibility.Visible;
            } else {
                C_BTN_Favorites_H.Visibility = System.Windows.Visibility.Collapsed;
                C_BTN_Favorites.Visibility = System.Windows.Visibility.Visible;
            }
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
            if (Environment.GetCommandLineArgs().Length > 1) {
                webControl.Source = new Uri(Environment.GetCommandLineArgs()[1]);
            } else if (Settings.Default.SessionRestore) {
                if (!String.IsNullOrEmpty(Settings.Default.LastUrl))
                    webControl.Source = new Uri(Settings.Default.LastUrl);
            } else
                webControl.GoToHome();
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (HasActiveDownloads()) {
                //Downloads are still running!
            }

            Settings.Default.LastUrl = webControl.Source.ToString();
            Settings.Default.Save();
        }

        private void webControl_ShowCreatedWebView(object sender, ShowCreatedWebViewEventArgs e) {
            if (webControl == null)
                return;

            if (!webControl.IsLive)
                return;

            e.Cancel = true;

            webControl.Source = e.TargetURL; //Force all popups into the main view (Temporary)
        }

        #region Keyboard Shortcuts

        private void webControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.KeyboardDevice.Modifiers == System.Windows.Input.ModifierKeys.Control) {
                switch (e.Key) {
                    case System.Windows.Input.Key.OemPlus:
                        webControl.ZoomIn();
                        break;
                    case System.Windows.Input.Key.OemMinus:
                        webControl.ZoomOut();
                        break;
                    case System.Windows.Input.Key.D0:
                        webControl.ResetZoom();
                        break;
                    case System.Windows.Input.Key.R:
                        C_BTN_Refresh_Click(this, null);
                        break;
                    case System.Windows.Input.Key.D:
                        C_BTN_Favorites_Click(this, null);
                        break;
                    case System.Windows.Input.Key.F5:
                        webControl.Reload(true);
                        break;
                }
            } else if (e.KeyboardDevice.Modifiers == System.Windows.Input.ModifierKeys.Alt) {
                switch (e.Key) {
                    case System.Windows.Input.Key.Home:
                        C_BTN_Home_Click(this, null);
                        break;
                    case System.Windows.Input.Key.BrowserHome:
                        C_BTN_Home_Click(this, null);
                        break;
                    case System.Windows.Input.Key.F4:
                        this.Close();
                        break;
                }
            }
        }

        #endregion
    }
}

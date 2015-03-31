using System;
using System.Windows;
using System.Windows.Media.Animation;
using Awesomium.Core;
using MahApps.Metro.Controls;
using NDatabase;
using Opeity.Components;
using Opeity.Components.Classes;
using Opeity.Properties;

namespace Opeity {
    public partial class MainWindow : MetroWindow {
        WebSession Session;

        #region Helper Methods

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

        public MainWindow() {
            InitializeComponent();

            Session = WebCore.Sessions[Profiler.UserProfileBase] ?? WebCore.CreateWebSession(Profiler.UserProfileBase, WebPreferences.Default);
            webControl.WebSession = Session;

            if (Environment.GetCommandLineArgs().Length > 1)
                webControl.Source = new Uri(Environment.GetCommandLineArgs()[1]);
            else
                webControl.Source = new Uri(Settings.Default.Home);

            LoadFavorites();
        }

        #region Window Browser Controls

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
            Storyboard board = new Storyboard();
            DoubleAnimation PR_S_In = new DoubleAnimation(0, 5, new Duration(TimeSpan.FromMilliseconds(150)));

            board.Children.Add(PR_S_In);
            Storyboard.SetTarget(PR_S_In, Prog);
            Storyboard.SetTargetProperty(PR_S_In, new PropertyPath("(Height)"));
            board.Begin();

            C_BTN_Refresh.Visibility = System.Windows.Visibility.Collapsed;
            C_BTN_Stop.Visibility = System.Windows.Visibility.Visible;
        }

        private void webControl_LoadingFrameComplete(object sender, FrameEventArgs e) {
            Storyboard board = new Storyboard();
            DoubleAnimation PR_S_Ou = new DoubleAnimation(5, 0, new Duration(TimeSpan.FromMilliseconds(150)));

            board.Children.Add(PR_S_Ou);
            Storyboard.SetTarget(PR_S_Ou, Prog);
            Storyboard.SetTargetProperty(PR_S_Ou, new PropertyPath("(Height)"));
            board.Begin();

            C_BTN_Stop.Visibility = System.Windows.Visibility.Collapsed;
            C_BTN_Refresh.Visibility = System.Windows.Visibility.Visible;

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
            C_BTN_Stop.Visibility = System.Windows.Visibility.Collapsed;
            C_BTN_Refresh.Visibility = System.Windows.Visibility.Visible;
        }

        private void webControl_Crashed(object sender, CrashedEventArgs e) {
            C_BTN_Stop.Visibility = System.Windows.Visibility.Collapsed;
            C_BTN_Refresh.Visibility = System.Windows.Visibility.Visible;
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

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            Settings.Default.Save(); //Catch-All Settings Save
        }
    }
}

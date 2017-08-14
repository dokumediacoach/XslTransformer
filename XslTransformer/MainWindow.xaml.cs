using MahApps.Metro.Controls;
using System.Windows.Controls;
using XslTransformer.Core;
using System;
using System.Windows.Navigation;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

namespace XslTransformer
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        #region Constructor

        /// <summary>
        /// Constructor, setting MainFrame Content
        /// listening out for transformation start and end events
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Adding content to frame
            MainFrame.Content = new MainPage();

            // Delegate when the MainWindow is loaded
            Loaded += MainWindow_Loaded;
        }

        /// <summary>
        /// When the MainWindow is loaded hook into
        /// LoadCompleted event for content of MainFrame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MainFrame.LoadCompleted += Nav_LoadCompleted;
        }

        /// <summary>
        /// When MainFrame load is completed and Content is MainPage
        /// hook into transformation start and end events invoked by MainViewModel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Nav_LoadCompleted(object sender, NavigationEventArgs e)
        {
            if (e.Content.GetType() != typeof(MainPage))
                return;
            MainViewModel mainVm = (e.Content as MainPage).DataContext as MainViewModel;
            mainVm.TransformationStart += MainVm_TransformationStart;
            mainVm.TransformationEnd += MainVm_TransformationEnd;
        }

        #endregion

        #region Transformation Running Events

        private void MainVm_TransformationStart(object sender, EventArgs e)
        {
            mTransformationRunning = true;
        }

        private void MainVm_TransformationEnd(object sender, EventArgs e)
        {
            mTransformationRunning = false;
        }

        private bool mTransformationRunning = false;

        #endregion

        #region Settings button

        /// <summary>
        /// Settings button click event
        /// </summary>
        /// <param name="sender">settings button</param>
        /// <param name="e"></param>
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (mTransformationRunning || mFrameNavigationRunning)
                return;
            mFrameNavigationRunning = true;
            if (MainFrame.Content.GetType() == typeof(MainPage))
                MainFrame.Navigate(new SettingsPage());
            else
                MainFrame.GoBack();
        }

        private bool mFrameNavigationRunning = false;

        #endregion

        #region MainFrame (Pages) Navigation

        private bool mAllowDirectNavigation = false;
        private NavigatingCancelEventArgs mNavArgs;

        private void MainFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (Content != null && !mAllowDirectNavigation)
            {
                e.Cancel = true;

                mNavArgs = e;

                // Create the storyboard
                var sb = new Storyboard();

                // Listen out for storyboard completed event, delegate to method
                sb.Completed += FadeOutCompleted;

                // Add fade out animation
                sb.AddFadeOut(400);

                // Start animating
                MainFrame.BeginStoryboard(sb);
            }
            mAllowDirectNavigation = false;
        }

        private async void FadeOutCompleted(object sender, EventArgs e)
        {
            mAllowDirectNavigation = true;
            switch (mNavArgs.NavigationMode)
            {
                case NavigationMode.New:
                    if (mNavArgs.Uri == null)
                        MainFrame.Navigate(mNavArgs.Content);
                    else
                        MainFrame.Navigate(mNavArgs.Uri);
                    break;
                case NavigationMode.Back:
                    if (MainFrame.CanGoBack)
                        MainFrame.GoBack();
                    break;
                case NavigationMode.Forward:
                    MainFrame.GoForward();
                    break;
                case NavigationMode.Refresh:
                    MainFrame.Refresh();
                    break;
            }

            await Dispatcher.BeginInvoke(DispatcherPriority.Loaded,
                (ThreadStart)delegate ()
                {
                    // Create the storyboard
                    var sb = new Storyboard();

                    // Add fade in animation
                    sb.AddFadeIn(400);

                    // Start animating
                    MainFrame.BeginStoryboard(sb);
                });
            mFrameNavigationRunning = false;
        }

        #endregion
    }
}

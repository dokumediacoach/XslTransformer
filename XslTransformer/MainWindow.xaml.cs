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

            // Adding main page content to frame
            MainFrame.Content = GetMainPage();

            // Delegate when the MainWindow is loaded
            Loaded += MainWindow_Loaded;
        }

        #endregion

        #region Event Helper Methods

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
        /// When MainFrame load is completed
        /// hook into events invoked by ViewModel of loaded page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Nav_LoadCompleted(object sender, NavigationEventArgs e)
        {
            // for MainPage
            if (e.Content.GetType() == typeof(MainPage))
            {
                MainViewModel mainVm = (e.Content as MainPage).DataContext as MainViewModel;
                mainVm.TransformationStart += MainVm_TransformationStart;
                mainVm.TransformationEnd += MainVm_TransformationEnd;
            }
            // for SettingsPage
            else
            {
                SettingsViewModel settingsVm = (e.Content as SettingsPage).DataContext as SettingsViewModel;
                settingsVm.BackToMainPage += SettingsVm_BackToMainPage;
            }
        }

        #endregion

        #region Transformation Running Events

        /// <summary>
        /// Method that is invoked if XSL Transformation starts in MainPage
        /// </summary>
        /// <param name="sender">instance that raises the event</param>
        /// <param name="e">event data</param>
        private void MainVm_TransformationStart(object sender, EventArgs e)
        {
            // set private member indicating that transformation is running to true
            mTransformationRunning = true;
        }

        /// <summary>
        /// Method that is invoked if XSL Transformation ends in MainPage
        /// </summary>
        /// <param name="sender">instance that raises the event</param>
        /// <param name="e">event data</param>
        private void MainVm_TransformationEnd(object sender, EventArgs e)
        {
            // set private member indicating that transformation is running to false
            mTransformationRunning = false;
        }

        /// <summary>
        /// Private member indicating if transformation is running in MainPage
        /// </summary>
        private bool mTransformationRunning = false;

        #endregion

        #region Back to Main Page Navigation

        /// <summary>
        /// Method that is invoked if user wants to get from settings page back to main page
        /// </summary>
        /// <param name="sender">instance that raises the event</param>
        /// <param name="e">event data</param>
        private void SettingsVm_BackToMainPage(object sender, EventArgs e)
        {
            // Perform MainFrame navigation
            MainFrame.Navigate(GetMainPage());
        }

        #endregion

        #region Settings Button

        /// <summary>
        /// Settings button click event
        /// </summary>
        /// <param name="sender">settings button</param>
        /// <param name="e"></param>
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Return if transformation or MainFrame navigation is running
            if (mTransformationRunning || mFrameNavigationRunning)
                return;

            // Indicate that MainFrame navigation is running
            mFrameNavigationRunning = true;

            // Navigate to the other page
            if (MainFrame.Content.GetType() == typeof(MainPage))
                MainFrame.Navigate(GetSettingsPage());
            else
                MainFrame.Navigate(GetMainPage());
        }

        /// <summary>
        /// Private member indicating if MainFrame navigation is running
        /// </summary>
        private bool mFrameNavigationRunning = false;

        #endregion

        #region MainFrame Content Helper

        /// <summary>
        /// Gets a MainPage
        /// </summary>
        /// <returns>MainPage instance</returns>
        private MainPage GetMainPage()
        {
            // Create new MainPage instance
            MainPage mainPage = new MainPage();

            // create Settings instance
            IReadSettings settings = new Settings();

            // create XmlProcessor instance
            IProcessXml xmlProcessor = new XmlCoreProcessor(settings);

            // Setting DataContext of MainPage to ViewModel, dependency injection of Settings and XmlProcessor
            mainPage.DataContext = new MainViewModel(settings, xmlProcessor);

            // Return MainPage instance
            return mainPage;
        }

        /// <summary>
        /// Gets a SettingsPage
        /// </summary>
        /// <returns>SettingsPage instance</returns>
        private SettingsPage GetSettingsPage()
        {
            // Create new SettingsPage instance
            SettingsPage settingsPage = new SettingsPage();

            // Setting DataContext of SettingsPage to ViewModel, dependency injection of Settings
            settingsPage.DataContext = new SettingsViewModel(new Settings());

            // Return SettingsPage instance
            return settingsPage;
        }

        #endregion

        #region MainFrame (Pages) Navigation Animation

        /// <summary>
        /// Private member ensuring that MainFrame.Navigate can not be called during running animation
        /// </summary>
        private bool mAllowDirectNavigation = false;

        /// <summary>
        /// Storing navigation event arguments between events
        /// </summary>
        private NavigatingCancelEventArgs mNavArgs;

        /// <summary>
        /// Method that is invoked when MainFrame navigation starts
        /// </summary>
        /// <param name="sender">MainFrame frame</param>
        /// <param name="e">navigation event arguments</param>
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

        /// <summary>
        /// Method that is invoked when the FadeOut animation in MainFrame is finished
        /// </summary>
        /// <param name="sender">instance that raises the event</param>
        /// <param name="e">event data</param>
        private async void FadeOutCompleted(object sender, EventArgs e)
        {
            mAllowDirectNavigation = true;

            // Navigate as set in navigation arguments
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

            // This ensures that first page is also faded in when program starts
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
            
            // Indicate that MainFrame navigation is finished
            mFrameNavigationRunning = false;
        }

        #endregion
    }
}

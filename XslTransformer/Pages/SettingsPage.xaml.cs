using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using XslTransformer.Core;

namespace XslTransformer
{
    /// <summary>
    /// Interaktionslogik für MainPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        #region Constructor

        /// <summary>
        /// Constructor with event handling for back to main page event
        /// </summary>
        public SettingsPage()
        {
            InitializeComponent();

            // set DataContext to SettingsViewModel
            DataContext = new SettingsViewModel(new Settings());

            // get SettingsViewModel
            SettingsViewModel vm = DataContext as SettingsViewModel;

            // Listen out for BackToMainPage event invoked by SettingsViewModel
            vm.BackToMainPage += SettingsViewModel_BackToMainPage;
        }

        #endregion

        #region Event Method

        private void SettingsViewModel_BackToMainPage(object sender, EventArgs e)
        {
            NavigationService nav = NavigationService.GetNavigationService(this);
            nav.GoBack();
        }

        #endregion
    }
}

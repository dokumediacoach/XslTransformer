using System.Resources;
using System.Windows.Controls;
using XslTransformer.Core;

namespace XslTransformer
{
    /// <summary>
    /// Interaktionslogik für MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            // By injecting TransientXslTransformerSettingsWrapper in MainViewModel DataContext
            // updated Settings from SettingsPage are retrieved.
            // In combination with the MainFrame go-back-navigation for MainPage (user input retained)
            // a more complex IoC / FactoryMethod solution can be spared.
            DataContext = new MainViewModel(new Settings());

            // when MainPage loaded – is also fired after Navigate.GoBack()
            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Settings constructor syncs settings from configuration so do nothing on first load
            if (mFirstLoad)
            {
                mFirstLoad = false;
                return;
            }
            // Resync settings from configuration when page is reloaded
            MainViewModel mainViewModel = DataContext as MainViewModel;
            if (mainViewModel.SyncSettingsFromConfigurationCommand.CanExecute(null))
                mainViewModel.SyncSettingsFromConfigurationCommand.Execute(null);
        }

        private bool mFirstLoad = true;
    }
}

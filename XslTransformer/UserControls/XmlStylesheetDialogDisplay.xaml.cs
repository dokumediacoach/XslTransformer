using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using XslTransformer.Core;

namespace XslTransformer.UserControls
{
    /// <summary>
    /// Interaktionslogik für CustomDialogDisplay.xaml
    /// </summary>
    public partial class XmlStylesheetDialogDisplay : UserControl
    {
        public XmlStylesheetDialogDisplay()
        {
            InitializeComponent();
        }

        #region private Members

        /// <summary>
        /// Holds reference to main MahApps MetroWindow where custom dialog gets displayed
        /// </summary>
        private MetroWindow metroWindow = Application.Current.MainWindow as MetroWindow;

        #endregion

        #region Public Dependency Properties

        /// <summary>
        /// Triggers display of a modal overlay custom dialog in the main window.
        /// Main window is blocked while it's displayed and cannot be moved or resized.
        /// </summary>
        public bool Show
        {
            get { return (bool)GetValue(ShowProperty); }
            set { SetValue(ShowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Show.  This enables animation, styling, binding, etc...
        public static volatile DependencyProperty ShowProperty = DependencyProperty.Register(
            "Show",
            typeof(bool),
            typeof(XmlStylesheetDialogDisplay),
            new FrameworkPropertyMetadata(
                   false,
                   FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                   OnShowPropertyChanged));


        /// <summary>
        /// XmlStylesheets list for binding data to display and return
        /// </summary>
        public ObservableCollection<XmlStylesheet> XmlStylesheets
        {
            get => GetValue(XmlStylesheetsProperty) as ObservableCollection<XmlStylesheet>;
            set => SetValue(XmlStylesheetsProperty, value);
        }

        // Using a DependencyProperty as the backing store for XmlStylesheets.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty XmlStylesheetsProperty = DependencyProperty.Register(
            "XmlStylesheets",
            typeof(ObservableCollection<XmlStylesheet>),
            typeof(XmlStylesheetDialogDisplay),
            new FrameworkPropertyMetadata(
                   null,
                   FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal));

        #endregion

        /// <summary>
        /// Async callback that is invoked when ShowProperty value changes
        /// </summary>
        /// <param name="d">DependencyObject (CustomDialogDisplay class)</param>
        /// <param name="e">Event data that is issued by ShowAsyncProperty value changes to the effective value of this property.</param>
        private static async void OnShowPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // return if value of ShowProperty (e.NewValue) is changed to false
            if (!(bool)e.NewValue)
                return;

            XmlStylesheetDialogDisplay display = (XmlStylesheetDialogDisplay)d;

            // let modal dialog overlap title bar of the main window so that it can be moved
            display.metroWindow.ShowDialogsOverTitleBar = false;

            // create dialog initializing XmlStlyesheets list in its ViewModel
            XmlStylesheetDialog dialog = new XmlStylesheetDialog(new XmlStylesheetDialogViewModel(display.XmlStylesheets));

            // show async dialog
            await display.metroWindow.ShowMetroDialogAsync(dialog);

            // on dialog confirmed event set XmlStylesheets to update XmlStylesheetDeclarations in MainViewModel
            dialog.DialogConfirmed += (sender, args) =>
            {
                // get dialog ViewModel
                XmlStylesheetDialogViewModel vm = (XmlStylesheetDialogViewModel)dialog.DataContext;
                // get XmlStylesheets from dialog ViewModel
                ObservableCollection<XmlStylesheet> xs = vm.XmlStylesheets;
                // update XmlStylesheets property
                display.XmlStylesheets = xs;
            };

            // wait until dialog is gone
            await dialog.WaitUntilUnloadedAsync();
            
            // reset Show property
            display.Show = false;
        }

    }
}

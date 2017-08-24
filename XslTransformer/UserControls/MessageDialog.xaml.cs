using System;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using XslTransformer.Core;

namespace XslTransformer.UserControls
{
    /// <summary>
    /// Interaktionslogik für MessageDialog.xaml
    /// </summary>
    public partial class MessageDialog : UserControl
    {
        #region Constructor

        public MessageDialog()
        {
            InitializeComponent();
        }

        #endregion

        #region private Members

        /// <summary>
        /// Holds reference to main MahApps MetroWindow where message dialog gets displayed
        /// </summary>
        private MetroWindow metroWindow = Application.Current.MainWindow as MetroWindow;

        #endregion

        #region Public Dependency Properties

        public static volatile DependencyProperty ShowAsyncProperty = DependencyProperty.Register(
               "ShowAsync",
               typeof(bool),
               typeof(MessageDialog),
               new FrameworkPropertyMetadata(
                   false,
                   FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                   OnShowAsyncPropertyChanged));

        public static DependencyProperty TitleProperty = DependencyProperty.Register(
               "Title",
               typeof(string),
               typeof(MessageDialog),
               new FrameworkPropertyMetadata(
                   null,
                   FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal));

        public static DependencyProperty TextProperty = DependencyProperty.Register(
               "Text",
               typeof(string),
               typeof(MessageDialog),
               new FrameworkPropertyMetadata(
                   null,
                   FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal));

        public static DependencyProperty IconProperty = DependencyProperty.Register(
               "Icon",
               typeof(MessageIcon),
               typeof(MessageDialog),
               new FrameworkPropertyMetadata(
                   MessageIcon.No,
                   FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal));


        #endregion

        #region Public Properties

        /// <summary>
        /// Triggers display of a async awaitable overlay message in the main window.
        /// Main window is not blocked while it's displayed and can still be moved and resized.
        /// </summary>
        public bool ShowAsync
        {
            get => (bool)GetValue(ShowAsyncProperty);
            set => SetValue(ShowAsyncProperty, value);
        }

        /// <summary>
        /// Title of the message to be displayed
        /// </summary>
        public string Title
        {
            get => GetValue(TitleProperty) as String;
            set => SetValue(TitleProperty, value);
        }

        /// <summary>
        /// Text content of the message to be displayed
        /// </summary>
        public string Text
        {
            get => GetValue(TextProperty) as String;
            set => SetValue(TextProperty, value);
        }

        /// <summary>
        /// Custom icon of the message to be displayed.
        /// </summary>
        public MessageIcon Icon
        {
            get => (MessageIcon)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        #endregion

        /// <summary>
        /// Async callback that is invoked when ShowAsyncProperty value changes
        /// </summary>
        /// <param name="d">DependencyObject (MessageDialog class)</param>
        /// <param name="e">Event data that is issued by ShowAsyncProperty value changes to the effective value of this property.</param>
        private static async void OnShowAsyncPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // return if value of ShowAsyncProperty (e.NewValue) is changed to false
            if (!(bool)e.NewValue)
                return;

            var dialog = (MessageDialog)d;

            // apply custom icon style
            dialog.ApplyCustomIconStyle();

            // don't let dialog overlap title bar of the main window so that it can be moved
            dialog.metroWindow.ShowDialogsOverTitleBar = false;

            // show MahApps message dialog
            await dialog.metroWindow.ShowMessageAsync(dialog.Title, dialog.Text);

            // reset Show to trigger messageShown event in MainWindowViewModel
            dialog.ShowAsync = false;
        }

        /// <summary>
        /// Apply current icon style to customized dialog
        /// </summary>
        private void ApplyCustomIconStyle()
        {
            // get style associated with current icon (located in /Resources/CustomizedDialogs.xaml)
            Style iconStyle = Application.Current.FindResource(Icon.ToString() + "MessageIcon") as Style;

            // set custom message icon style (DynamicResource in XAML) to associated icon style
            Application.Current.Resources["CustomMessageIconStyle"] = iconStyle;
        }
    }
}

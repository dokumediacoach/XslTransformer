using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;

namespace XslTransformer.UserControls
{
    /// <summary>
    /// Interaktionslogik für XmlInputFilePicker.xaml
    /// </summary>
    public partial class XmlInputFilePicker : UserControl
    {
        public XmlInputFilePicker()
        {
            InitializeComponent();
        }

        #region Public Properties

        /// <summary>
        /// Property for displaying loaded XmlInputPath in readonly TextBox.
        /// XmlInputText is stored in DependencyProperty XmlInputTextProperty.
        /// Property gets set by MainWindowViewModel.
        /// </summary>
        public string XmlInputText
        {
            get => GetValue(XmlInputTextProperty) as String;
            set => SetValue(XmlInputTextProperty, value);
        }

        /// <summary>
        /// Property for triggering the loading of XmlInputPath.
        /// XmlInputFile is stored in DependencyProperty XmlInputFileProperty.
        /// Property is read by MainWindowViewModel.
        /// </summary>
        public string XmlInputFile
        {
            get => GetValue(XmlInputFileProperty) as String;
            set => SetValue(XmlInputFileProperty, value);
        }

        /// <summary>
        /// Property for title of input file picker dialog.
        /// DialogTitle is stored in DependencyProperty DialogTitleProperty.
        /// Property gets set by MainWindowViewModel.
        /// </summary>
        public string DialogTitle
        {
            get => GetValue(DialogTitleProperty) as String;
            set => SetValue(DialogTitleProperty, value);
        }

        #endregion

        #region Public Dependency Properties


        public static DependencyProperty XmlInputTextProperty = DependencyProperty.Register(
               "XmlInputText",
               typeof(string),
               typeof(XmlInputFilePicker),
               new FrameworkPropertyMetadata(
                   null,
                   FrameworkPropertyMetadataOptions.Journal));

        
        public static readonly DependencyProperty XmlInputFileProperty = DependencyProperty.Register(
                "XmlInputFile",
                typeof(string),
                typeof(XmlInputFilePicker),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal));


        public static readonly DependencyProperty DialogTitleProperty = DependencyProperty.Register(
                "DialogTitle",
                typeof(string),
                typeof(XmlInputFilePicker),
                new FrameworkPropertyMetadata(
                   String.Empty,
                   FrameworkPropertyMetadataOptions.Journal));

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = DialogTitle;

            if (openFileDialog.ShowDialog() == true)
            {
                XmlInputFile = openFileDialog.FileName;
            }
        }
    }
}

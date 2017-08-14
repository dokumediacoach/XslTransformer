using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace XslTransformer.UserControls
{
    /// <summary>
    /// Interaktionslogik für XslInputFilesPicker.xaml
    /// </summary>
    public partial class XslInputFilesPicker : UserControl
    {
        public XslInputFilesPicker()
        {
            InitializeComponent();
        }

        #region Public Properties

        /// <summary>
        /// Property for the label text of the button.
        /// XslInputLabel is stored in DependencyProperty XslInputLabelProperty.
        /// Property gets set by MainWindowViewModel.
        /// </summary>
        public string XslInputLabel
        {
            get => GetValue(XslInputLabelProperty) as String;
            set => SetValue(XslInputLabelProperty, value);
        }

        /// <summary>
        /// Property for triggering the loading of new Stylesheets.
        /// "XslInputFileList" is stored in DependencyProperty XslInputFileListProperty.
        /// Property is read by MainWindowViewModel.
        /// </summary>
        public ObservableCollection<string> XslInputFileList
        {
            get => GetValue(XslInputFileListProperty) as ObservableCollection<string>;
            set => SetValue(XslInputFileListProperty, value);
        }

        /// <summary>
        /// Property for title of xsl input files picker dialog.
        /// DialogTitle is stored in DependencyProperty DialogTitleProperty.
        /// Property gets set by MainWindowViewModel.
        /// </summary>
        public string DialogTitle
        {
            get => GetValue(DialogTitleProperty) as String;
            set => SetValue(DialogTitleProperty, value);
        }

        #endregion

        #region DependencyProperties

        
        public static DependencyProperty XslInputLabelProperty = DependencyProperty.Register(
               "XslInputLabel",
               typeof(string),
               typeof(XslInputFilesPicker),
               new FrameworkPropertyMetadata(
                   null,
                   FrameworkPropertyMetadataOptions.Journal));

        
        public static readonly DependencyProperty XslInputFileListProperty = DependencyProperty.Register(
                "XslInputFileList",
                typeof(ObservableCollection<string>),
                typeof(XslInputFilesPicker),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal));


        public static readonly DependencyProperty DialogTitleProperty = DependencyProperty.Register(
                "DialogTitle",
                typeof(string),
                typeof(XslInputFilesPicker),
                new FrameworkPropertyMetadata(
                   String.Empty,
                   FrameworkPropertyMetadataOptions.Journal));

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = DialogTitle;
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == true)
            {
                ObservableCollection<string> xslInputFileList = new ObservableCollection<string>();
                foreach (string file in openFileDialog.FileNames)
                {
                    xslInputFileList.Add(file);
                }
                XslInputFileList = xslInputFileList;
            }
        }
    }
}

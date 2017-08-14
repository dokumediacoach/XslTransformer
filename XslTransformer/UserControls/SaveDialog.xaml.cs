using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

namespace XslTransformer.UserControls
{
    /// <summary>
    /// Interaktionslogik für SaveDialog.xaml
    /// </summary>
    public partial class SaveDialog : UserControl
    {
        public SaveDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Property for title of output save file dialog.
        /// Title is stored in DependencyProperty TitleProperty.
        /// Property gets set by MainWindowViewModel.
        /// </summary>
        public string Title
        {
            get => GetValue(TitleProperty) as String;
            set => SetValue(TitleProperty, value);
        }

        public static DependencyProperty TitleProperty = DependencyProperty.Register(
                "Title",
                typeof(string),
                typeof(SaveDialog),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal));


        /// <summary>
        /// Property for triggering the saving to OutputFilePath.
        /// SaveFilePath is stored in DependencyProperty SaveFilePathProperty.
        /// Property is read by MainWindowViewModel.
        /// </summary>
        public string SaveFilePath
        {
            get => GetValue(SaveFilePathProperty) as String;
            set => SetValue(SaveFilePathProperty, value);
        }

        public static DependencyProperty SaveFilePathProperty = DependencyProperty.Register(
                "SaveFilePath",
                typeof(string),
                typeof(SaveDialog),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal));


        /// <summary>
        /// Property that triggers modal save file dialog for transformation output.
        /// FilePathProposal is stored in DependencyProperty FilePathProposalProperty.
        /// Property gets set by MainWindowViewModel.
        /// </summary>
        public string FilePathProposal
        {
            get => GetValue(FilePathProposalProperty) as String;
            set => SetValue(FilePathProposalProperty, value);
        }

        public static DependencyProperty FilePathProposalProperty = DependencyProperty.Register(
                    "FilePathProposal",
                    typeof(string),
                    typeof(SaveDialog),
                    new FrameworkPropertyMetadata(
                        String.Empty,
                        FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                        OnFilePathProposalPropertyChanged));

        
        private static void OnFilePathProposalPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // return if value of FilePathProposal (e.NewValue) is changed to null or empty string
            if (String.IsNullOrEmpty(e.NewValue as String))
                return;

            SaveDialog dialog = (SaveDialog)d;

            // create SaveFileDialog with initial settings from SaveDialog properties
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(dialog.FilePathProposal);
            saveFileDialog.FileName = System.IO.Path.GetFileName(dialog.FilePathProposal);
            saveFileDialog.Title = dialog.Title;

            // Show dialog, store result
            bool? result = saveFileDialog.ShowDialog();

            // If user did not cancel set SaveFilePath property to trigger saving in view model
            dialog.SaveFilePath = (result ?? false) ? saveFileDialog.FileName : String.Empty;
        }
    }
}

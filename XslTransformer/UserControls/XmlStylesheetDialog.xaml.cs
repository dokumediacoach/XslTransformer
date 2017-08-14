using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using XslTransformer.Core;

namespace XslTransformer.UserControls
{
    /// <summary>
    /// Interaktionslogik für XmlStylesheetMediaDialog.xaml
    /// </summary>
    public partial class XmlStylesheetDialog : CustomDialog
    {
        /// <summary>
        /// Contructor injecting data context
        /// </summary>
        /// <param name="vm"></param>
        public XmlStylesheetDialog(XmlStylesheetDialogViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        /// <summary>
        /// Holds reference to MetroWindow for MetroDialog handling
        /// </summary>
        private MetroWindow metroWindow = Application.Current.MainWindow as MetroWindow;

        /// <summary>
        /// Gets raised when user confirms or cancels the dialog
        /// </summary>
        public event EventHandler DialogConfirmed;

        /// <summary>
        /// If user clicked row in DataGrid
        /// </summary>
        /// <param name="sender">the DataGridRow</param>
        /// <param name="e">mouse event arguments</param>
        private void DataGridRow_MouseUp(object sender, MouseEventArgs e)
        {
            // toggle CheckBox in row
            ToggleRowCheckBox(sender as DataGridRow);

            // set event handled
            e.Handled = true;
        }

        /// <summary>
        /// If user pressed a key when row in DataGrid has focus
        /// </summary>
        /// <param name="sender">the DataGridRow</param>
        /// <param name="e">key event arguments</param>
        private void DataGridRow_KeyDown(object sender, KeyEventArgs e)
        {
            // if key is not space key return
            if (e.Key != Key.Space) return;

            // toggle CheckBox in row
            ToggleRowCheckBox(sender as DataGridRow);

            // set event handled
            e.Handled = true;
        }

        /// <summary>
        /// Toggle CheckBox of a row in DataGrid
        /// </summary>
        /// <param name="row">the DataGridRow where CheckBox shall be toggled</param>
        private void ToggleRowCheckBox(DataGridRow row)
        {
            // get CheckBox from CheckBoxColumn (name == null)
            CheckBox checkBox = row.FindChild<CheckBox>(null);

            // toggle CheckBox
            checkBox.IsChecked ^= true;
        }

        /// <summary>
        /// If user clicked Yes-Button
        /// </summary>
        /// <param name="sender">the button</param>
        /// <param name="e">the routed event arguments</param>
        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            RaiseDialogConfirmedEvent();
            metroWindow.HideMetroDialogAsync(this);
        }

        /// <summary>
        /// If user clicked No-Button
        /// </summary>
        /// <param name="sender">the button</param>
        /// <param name="e">the routed event arguments</param>
        private void DontApply_Click(object sender, RoutedEventArgs e)
        {
            // Get ViewModel from DataContext
            XmlStylesheetDialogViewModel vm = (XmlStylesheetDialogViewModel)DataContext;
            
            // Iterate over XmlStylesheets in ViewModel, deselecting all stylesheets
            foreach (XmlStylesheet xs in vm.XmlStylesheets)
            {
                xs.IsSelected = false;
            }
            RaiseDialogConfirmedEvent();
            metroWindow.HideMetroDialogAsync(this);
        }

        /// <summary>
        /// Invokes dialog confirmed event
        /// </summary>
        protected virtual void RaiseDialogConfirmedEvent()
        {
            DialogConfirmed?.Invoke(this, EventArgs.Empty);
        }
    }
}

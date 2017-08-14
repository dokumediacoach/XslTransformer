using MahApps.Metro.Controls.Dialogs;
using XslTransformer.Core;

namespace XslTransformer
{
    class ErrorDialogViewModel : BaseViewModel
    {
        /// <summary>
        /// Stores reference variable to IDialogCoordinator
        /// </summary>
        private IDialogCoordinator mDialogCoordinator;

        // Constructor
        public ErrorDialogViewModel(IDialogCoordinator instanceOfIDialogCoordinator)
        {
            mDialogCoordinator = instanceOfIDialogCoordinator;
        }

    }
}

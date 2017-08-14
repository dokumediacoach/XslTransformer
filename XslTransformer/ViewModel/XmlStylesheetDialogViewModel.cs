using System.Collections.ObjectModel;
using System.Windows.Input;
using XslTransformer.Core;

namespace XslTransformer
{
    public class XmlStylesheetDialogViewModel : BaseViewModel
    {
        /// <summary>
        /// Constructor inputting XmlStylesheets
        /// </summary>
        /// <param name="xmlStylesheets">List of XSLT Stlyesheets derived from xml-stylesheet declarations in XML input file</param>
        public XmlStylesheetDialogViewModel(ObservableCollection<XmlStylesheet> xmlStylesheets)
        {
            XmlStylesheets = xmlStylesheets;
        }

        /// <summary>
        /// List of XSLT Stlyesheets derived from xml-stylesheet declarations in XML input file
        /// </summary>
        public ObservableCollection<XmlStylesheet> XmlStylesheets { get; set; }
    }
}

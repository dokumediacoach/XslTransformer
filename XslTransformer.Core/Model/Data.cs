using System.Collections.ObjectModel;

namespace XslTransformer.Core
{
    public static class Data
    {
        /// <summary>
        /// The path to the XML input file
        /// </summary>
        public static string XmlInputPath { get; set; }

        /// <summary>
        /// List of XSLT Stylesheets to be applied to XML input file
        /// </summary>
        public static ObservableCollection<XsltStylesheet> Stylesheets { get; set; } = new ObservableCollection<XsltStylesheet>();
    }
}

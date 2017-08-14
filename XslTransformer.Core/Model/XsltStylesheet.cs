using System.Collections.ObjectModel;

namespace XslTransformer.Core
{
    public class XsltStylesheet
    {
        // Path to Stylesheet
        public string Path { get; set; }

        // Parameters for XSLT Stylesheet
        public ObservableCollection<StylesheetParameter> Parameters { get; set; } = new ObservableCollection<StylesheetParameter>();
    }
}

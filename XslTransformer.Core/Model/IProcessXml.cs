using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XslTransformer.Core
{
    public interface IProcessXml
    {
        /// <summary>
        /// Parses xml file to check it for errors.
        /// </summary>
        /// <param name="xmlFilePath">path to xml file</param>
        /// <returns>awaitable Task with XmlStylesheetDeclarations found in xml file or null if errors occured</returns>
        Task<ObservableCollection<XmlStylesheet>> CheckXmlFile(string xmlFilePath);

        /// <summary>
        /// Loads xslt file to check it for errors.
        /// </summary>
        /// <param name="stylesheetFilePath">path to xslt file</param>
        /// <returns>Task with true if file is okay, false if errors occured</returns>
        Task<bool> CheckStylesheet(string stylesheetFilePath);

        /// <summary>
        /// Performs xsl transformation after static Data.
        /// </summary>
        /// <returns>awaitable Task with output file name proposal for transformation result or null if errors occured</returns>
        Task<string> Transform();

        /// <summary>
        /// Holds Xsl Transformation result in memory
        /// </summary>
        MemoryStream OutputStream { get; }

        /// <summary>
        /// Stores output encoding
        /// </summary>
        Encoding OutputEncoding { get; }

        /// <summary>
        /// Event that gets invoked when error or warning messages are to be displayed
        /// </summary>
        event EventHandler<MessageEventArgs> ShowAsyncMessage;

        /// <summary>
        /// When async warning or error message is shown this AutoResetEvent is triggered
        /// </summary>
        AutoResetEvent AsyncMessageShown { get; set; }
    }
}

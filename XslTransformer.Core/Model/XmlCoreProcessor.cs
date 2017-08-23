using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Xsl;

namespace XslTransformer.Core
{
    public class XmlCoreProcessor : IProcessXml
    {
        /// <summary>
        /// Default constructor, injecting settings
        /// </summary>
        /// <param name="settings">settings to be read</param>
        public XmlCoreProcessor(IReadSettings settings)
        {
            mSettings = settings;
            mXmlReaderSettings = CreateXmlReaderSettings();
            mXsltSettings = CreateXsltSettings();
            mXmlUrlResolver = CreateXmlUrlResolver();
        }

        /// <summary>
        /// Holds DI reference to XslTransformerSettings for reading
        /// </summary>
        private readonly IReadSettings mSettings;

        /// <summary>
        /// XmlReaderSettings according to current XslTransformerSettings
        /// </summary>
        private XmlReaderSettings mXmlReaderSettings;

        /// <summary>
        /// XsltSettings according to current XslTransformerSettings
        /// </summary>
        private XsltSettings mXsltSettings;

        /// <summary>
        /// Default XmlUrlResolver
        /// </summary>
        private XmlUrlResolver mXmlUrlResolver;

        /// <summary>
        /// Memory stream for transformation output
        /// </summary>
        public MemoryStream OutputStream { get; private set; }

        /// <summary>
        /// Stores output encoding
        /// </summary>
        public Encoding OutputEncoding { get; private set; }

        /// <summary>
        /// Event that gets invoked when error or warning messages are to be displayed
        /// </summary>
        public event EventHandler<MessageEventArgs> ShowAsyncMessage;

        /// <summary>
        /// When async warning or error message is shown this AutoResetEvent is triggered
        /// </summary>
        public AutoResetEvent AsyncMessageShown { get; set; } = new AutoResetEvent(false);

        /// <summary>
        /// Parses xml file to check it for errors and find xml-stylesheet declarations.
        /// </summary>
        /// <param name="xmlFilePath">path to xml file</param>
        /// <returns>awaitable Task with XmlStylesheetDeclarations found in xml file</returns>
        public async Task<ObservableCollection<XmlStylesheet>> CheckXmlFile(string xmlFilePath)
        {
            // Show message and return null if file does not exist
            if (!File.Exists(xmlFilePath))
            {
                await Message(MessageType.FileNotFoundError, xmlFilePath);
                return null;
            }

            // Create the XmlReader object
            XmlReader reader = XmlReader.Create(xmlFilePath, mXmlReaderSettings);

            // Create the object to collect xml-stylesheet declarations
            ObservableCollection<XmlStylesheet> xmlStylesheetDeclarations = new ObservableCollection<XmlStylesheet>();

            // Search for xml-stylesheet declarations
            bool searchXmlStylesheet = true;

            // Try to parse the file and pick xml-stylesheet declarations
            try
            {
                while (await reader.ReadAsync())
                {
                    // search for xml-stylesheet declarations if set so
                    if (!searchXmlStylesheet)
                        continue;

                    // stop searching for xml-stylesheet declarations when root element is reached
                    if (reader.NodeType == XmlNodeType.Document)
                    {
                        searchXmlStylesheet = false;
                        continue;
                    }

                    // try next node if current node is not a processing instruction named xml-stylesheet
                    if (reader.NodeType != XmlNodeType.ProcessingInstruction
                        || reader.Name != "xml-stylesheet")
                        continue;

                    // create a XML document as a helper to access xml-stylesheet-declaration attributes
                    XmlDocument stylesheet = new XmlDocument();

                    // load text value into stylesheet document so XPath can be used
                    stylesheet.LoadXml("<stylesheet " + reader.Value + "/>");

                    // check if xml-stylesheet has a href attribute and the right type attribute value ...
                    XmlNode stylesheetNode = stylesheet.SelectSingleNode("/stylesheet[@href][@type='text/xsl']");

                    // ... if not continue with next node
                    if (stylesheetNode == null)
                        continue;

                    // create xml stylesheet item with Href
                    XmlStylesheet xmlStylesheetItem = new XmlStylesheet()
                    {
                        Href = stylesheetNode.SelectSingleNode("@href").Value
                    };

                    // check if media attribute is present
                    string mediaAttributeValue = stylesheetNode.SelectSingleNode("@media")?.Value;

                    // if then add it to item
                    if (mediaAttributeValue != null)
                        xmlStylesheetItem.Media = mediaAttributeValue;

                    // add the item to the XmlStylesheetDeclarations list
                    xmlStylesheetDeclarations.Add(xmlStylesheetItem);
                }
            }
            catch (XmlException e)
            {
                await Message(MessageType.XmlInputFileMalformedXmlError, xmlFilePath, e.Message);
                return null;
            }
            catch (XmlSchemaValidationException e)
            {
                await Message(MessageType.XmlInputFileInvalidXmlError, xmlFilePath, e.Message);
                return null;
            }
            catch (Exception e)
            {
                await Message(MessageType.XmlInputFileError, xmlFilePath, e.Message);
                return null;
            }

            // return collected xml-stylesheet declarations
            return xmlStylesheetDeclarations;
        }

        /// <summary>
        /// Loads xslt file to check it for errors.
        /// </summary>
        /// <param name="stylesheetFilePath">path to xslt file</param>
        /// <returns>Task with true if file is okay, false if errors occured</returns>
        public async Task<bool> CheckStylesheet(string stylesheetFilePath)
        {
            // Show message and return false if xslt file does not exist
            if (!File.Exists(stylesheetFilePath))
            {
                await Message(MessageType.FileNotFoundError, stylesheetFilePath);
                return false;
            }

            try
            {
                // Create a XslCompiledTransform instance and load the stylesheet
                XslCompiledTransform xslt = new XslCompiledTransform();
                xslt.Load(stylesheetFilePath, mXsltSettings, mXmlUrlResolver);
            }
            catch (XmlException e)
            {
                await Message(MessageType.XsltMalformedXmlError, stylesheetFilePath, e.Message);
                return false;
            }
            catch (XsltException e)
            {
                await Message(MessageType.XsltStylesheetError, stylesheetFilePath, e.Message);
                return false;
            }
            catch (Exception e)
            {
                await Message(MessageType.XsltFileError, stylesheetFilePath, e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Performs xsl transformation after static Data.
        /// </summary>
        /// <returns>awaitable Task with output file name proposal for transformation result or null if errors occured</returns>
        public async Task<string> Transform()
        {
            // Check xml input file
            ObservableCollection<XmlStylesheet> checkReturn = await CheckXmlFile(Data.XmlInputPath);

            // return if errors occured
            if (checkReturn == null)
                return null;

            // Create the XmlReader instance for input file
            XmlReader inputReader = XmlReader.Create(Data.XmlInputPath, mXmlReaderSettings);

            // Create argument list for XSLT parameters
            XsltArgumentList arguments = new XsltArgumentList();

            // initialize XslCompiledTransform instance
            XslCompiledTransform xslt = null;

            // Recreate memory stream for transformation output
            OutputStream?.Close();
            OutputStream = new MemoryStream();

            // Create memory stream for transformation input
            MemoryStream inputStream = new MemoryStream();

            // Initialize path variable to remember last processed stylesheet
            string lastProcessedStylesheet = String.Empty;

            // iterate over stylesheet list
            bool stylesheetOkay;
            bool first = true;
            foreach (XsltStylesheet stylesheet in Data.Stylesheets)
            {
                // Check xslt file
                stylesheetOkay = await CheckStylesheet(stylesheet.Path);
                if (!stylesheetOkay)
                {
                    inputStream.Close();
                    OutputStream.Close();
                    return null;
                }

                // Create new XslCompiledTransform instance and load the stylesheet
                xslt = new XslCompiledTransform();
                xslt.Load(stylesheet.Path, mXsltSettings, mXmlUrlResolver);

                // empty out xslt arguments
                arguments.Clear();

                // iterate over parameter list
                foreach (StylesheetParameter parameter in stylesheet.Parameters)
                {
                    // add parameter to argument list
                    arguments.AddParam(parameter.Name, String.Empty, parameter.Value);
                }

                // input stream is used if there are more than one stylesheet in the list
                if (!first)
                {
                    // dispose and recreate input stream
                    inputStream.Close();
                    inputStream = new MemoryStream();

                    // copy output stream to input stream
                    OutputStream.WriteTo(inputStream);

                    // dispose and recreate output stream
                    OutputStream.Close();
                    OutputStream = new MemoryStream();

                    // reset input stream position to read from the beginning
                    inputStream.Seek(0, SeekOrigin.Begin);

                    // recreate input reader from input stream with settings
                    inputReader = XmlReader.Create(inputStream, mXmlReaderSettings);

                    // try to parse the xml in stream to test it
                    try
                    {
                        while (inputReader.Read()) ;
                    }
                    catch (Exception e)
                    {
                        await Message(MessageType.XslTransformationResultError, lastProcessedStylesheet, e.Message);
                        inputStream.Close();
                        OutputStream.Close();
                        return null;
                    }

                    // reset input stream position to read from the beginning
                    inputStream.Seek(0, SeekOrigin.Begin);

                    // recreate input reader to read from the beginning
                    inputReader = XmlReader.Create(inputStream, mXmlReaderSettings);
                }

                // Try to apply the transformation, put result to output stream
                try
                {
                    xslt.Transform(inputReader, arguments, OutputStream);
                }
                catch (Exception e)
                {
                    await Message(MessageType.XslTransformationError, stylesheet.Path, e.Message);
                    inputStream.Close();
                    OutputStream.Close();
                    return null;
                }

                // remember last processed stylesheets for possible error messages
                lastProcessedStylesheet = stylesheet.Path;

                // reset output stream position to read from the beginning
                OutputStream.Seek(0, SeekOrigin.Begin);

                if (first)
                    first = false;
            }

            // close input stream as it is no longer needed
            inputStream.Close();

            // Store output encoding
            OutputEncoding = xslt.OutputSettings.Encoding;

            // Propose input directory as output directory
            string outputDirectoryProposal = Path.GetDirectoryName(Data.XmlInputPath);

            // Get xml input file name without extension
            string inputFileNameWithoutExtension = Path.GetFileNameWithoutExtension(Data.XmlInputPath);

            // Get output method (Xml | Html | Text)
            XmlOutputMethod outputMethod = xslt.OutputSettings.OutputMethod;

            // Make extension proposal dependant from output method
            string extensionProposal = String.Empty;
            switch (outputMethod)
            {
                case XmlOutputMethod.Html:
                    extensionProposal = "html";
                    break;
                case XmlOutputMethod.Text:
                    extensionProposal = "txt";
                    break;
                default:
                    extensionProposal = "xml";
                    break;
            }

            // return concatenated output file name proposal
            return inputFileNameWithoutExtension + ".out." + extensionProposal;
        }

        /// <summary>
        /// Create XmlReaderSettings according to current XslTransformerSettings
        /// </summary>
        /// <returns>a new XmlReaderSettings instance</returns>
        private XmlReaderSettings CreateXmlReaderSettings()
        {
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();

            // Make async XmlReader methods available
            xmlReaderSettings.Async = true;

            // Set the reader settings object to use the default resolver.
            xmlReaderSettings.XmlResolver = CreateXmlUrlResolver();

            // set Properties according to current XslTransformerSettings

            DtdProcessing dtdProcessing;
            Enum.TryParse(mSettings.GetValue<XmlReaderDtdProcessing>(Setting.DtdProcessing).ToString(), out dtdProcessing);
            xmlReaderSettings.DtdProcessing = dtdProcessing;

            ValidationType validationType;
            Enum.TryParse(mSettings.GetValue<XmlReaderValidationType>(Setting.ValidationType).ToString(), out validationType);
            xmlReaderSettings.ValidationType = validationType;

            if (!mSettings.GetValue<bool>(Setting.CheckCharacters))
                xmlReaderSettings.CheckCharacters = false;

            // if validation is not requested return settings
            if (xmlReaderSettings.ValidationType == ValidationType.None)
                return xmlReaderSettings;

            // set ValidationFlags according to current XslTransformerSettings

            xmlReaderSettings.ValidationFlags = XmlSchemaValidationFlags.None;

            if (mSettings.GetValue<bool>(Setting.XsdValidationFlag_AllowXmlAttributes))
                xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.AllowXmlAttributes;

            if (mSettings.GetValue<bool>(Setting.XsdValidationFlag_ProcessIdentityConstraints))
                xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ProcessIdentityConstraints;

            if (mSettings.GetValue<bool>(Setting.XsdValidationFlag_ProcessSchemaLocation))
                xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;

            if (mSettings.GetValue<bool>(Setting.XsdValidationFlag_ProcessInlineSchema))
                xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;

            // Validation warnings don't throw exceptions ...
            if (mSettings.GetValue<bool>(Setting.XsdValidationFlag_ReportValidationWarnings))
            {
                xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                // ... but they can be displayed by using a ValidationEventHandler callback
                xmlReaderSettings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);
            }

            return xmlReaderSettings;
        }

        // ValidationEventHandler callback (strangely enough, message overlay seems stuck for seconds before message displays)
        private async void ValidationCallBack(object sender, ValidationEventArgs e)
        {
            // If a ValidationEventHandler is registered also ValidationErrors don't throw exceptions
            // so a distinction is necessary here
            if (e.Severity == XmlSeverityType.Warning)
                await Message(MessageType.XmlValidationWarning, null, e.Exception.Message);
            else
                await Message(MessageType.XmlValidationError, null, e.Exception.Message);
        }

        /// <summary>
        /// Create XsltSettings according to current XslTransformerSettings
        /// </summary>
        /// <returns>a new XsltSettings instance</returns>
        private XsltSettings CreateXsltSettings() => new XsltSettings(
            mSettings.GetValue<bool>(Setting.EnableDocumentFunction),
            mSettings.GetValue<bool>(Setting.EnableScript));

        /// <summary>
        /// Create a xml url resolver with default credentials
        /// </summary>
        /// <returns>a new default XmlUrlResolver instance</returns>
        private XmlUrlResolver CreateXmlUrlResolver() => new XmlUrlResolver
        {
            Credentials = CredentialCache.DefaultCredentials
        };

        /// <summary>
        /// Invokes event to display message and waits for auto reset event that is fired when message is displayed.
        /// </summary>
        /// <param name="messageType">The type of message to be displayed</param>
        /// <param name="filePath">the optional path to the processed file where error occured</param>
        /// <param name="message">the exception message to be included in message display</param>
        /// <returns></returns>
        private async Task Message(MessageType messageType, string filePath = null, string message = null)
        {
            ShowAsyncMessage?.Invoke(null, new MessageEventArgs
            {
                MessageType = messageType,
                FilePath = filePath,
                Message = message
            });
            await Task.Run(() =>
            {
                AsyncMessageShown.WaitOne();
            });
            return;
        }
    }
}

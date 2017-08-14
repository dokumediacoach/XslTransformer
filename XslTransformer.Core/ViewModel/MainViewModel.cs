using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;
using System.Xml.Schema;
using PropertyChanged;
using System.Threading;
using System.Net;
using System.Xml.Xsl;
using System.Text;
using System.Resources;
using System.Reflection;

namespace XslTransformer.Core
{
    /// <summary>
    /// View model for main application page
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        #region Private Members

        /// <summary>
        /// Stores reference to string resources for messages etc.
        /// </summary>
        private static ResourceManager mStrings = new ResourceManager("XslTransformer.Core.Strings.Resources", Assembly.GetExecutingAssembly());

        /// <summary>
        /// Holds DI reference to Settings for reading
        /// </summary>
        private readonly IReadSettings mSettings;

        /// <summary>
        /// Stores the index of the selected XSLT Stylesheet in the stylesheet list
        /// </summary>
        private int mSelectedStylesheetIndex;

        /// <summary>
        /// Stores show value for awaitable async message
        /// </summary>
        private bool mShowAsyncMessage = false;

        /// <summary>
        /// when async message dialog is shown and property ShowAsyncMessage is set to false this AutoResetEvent is triggered
        /// </summary>
        private AutoResetEvent asyncMessageShown = new AutoResetEvent(false);

        /// <summary>
        /// Stores show value for awaitable async xml-stylesheet dialog
        /// </summary>
        private bool mShowAsyncXmlStylesheetDialog = false;

        /// <summary>
        /// when async custom dialog is shown and property ShowAsyncCustomDialog is set to false this AutoResetEvent is triggered
        /// </summary>
        private AutoResetEvent asyncXmlStylesheetDialogShown = new AutoResetEvent(false);

        /// <summary>
        /// Stores value for the path to the output file of the transformation
        /// </summary>
        private string mOutputFilePath = String.Empty;

        /// <summary>
        /// When output file is selected in save file dialog and property OutputFile is set this AutoResetEvent is triggered 
        /// </summary>
        private AutoResetEvent outputFileSelected = new AutoResetEvent(false);

        #endregion

        #region Public Properties

        /// <summary>
        /// The path to the XML input file
        /// </summary>
        public string XmlInputPath { get; private set; }

        /// <summary>
        /// Path to a new XML input file, gets set from view
        /// </summary>
        public string XmlInputFile
        {
            set
            {
                if (value == null || TransformationIsRunning)
                    return;

                CheckAndSetXmlInputPath(value);
            }
        }

        /// <summary>
        /// List of XSLT Stylesheets to be applied to XML input file
        /// </summary>
        public ObservableCollection<XsltStylesheet> Stylesheets { get; private set; } = new ObservableCollection<XsltStylesheet>();

        /// <summary>
        /// New List of XSL files to be input, gets set from view
        /// </summary>
        public ObservableCollection<string> XslInputFileList
        {
            set
            {
                if (value == null || TransformationIsRunning)
                    return;

                CheckAndInputStylesheets(value);
            }
        }

        /// <summary>
        /// The currently selected Stylesheet
        /// </summary>
        [AlsoNotifyFor("IsTransformable")]
        public XsltStylesheet SelectedStylesheet { get; set; }

        /// <summary>
        /// Indicates if an XSLT Stylesheet is selected so that Parameters can be added and it can be deleted from the stylesheet list
        /// </summary>
        public bool IsStylesheetSelected => (SelectedStylesheet == null) ? false : true;

        /// <summary>
        /// The index of the selected XSLT Stylesheet in the stylesheet list.
        /// Setting this property moves it up and down in the list.
        /// </summary>
        public int SelectedStylesheetIndex
        {
            get => mSelectedStylesheetIndex = Stylesheets.IndexOf(SelectedStylesheet);
            private set
            {
                if (mSelectedStylesheetIndex == value || value < 0 || value > (Stylesheets.Count - 1))
                    return;
                Stylesheets.Move(mSelectedStylesheetIndex, value);
                mSelectedStylesheetIndex = value;
            }
        }

        /// <summary>
        /// Indicates if an XSLT Stylesheet can be moved upwards in the stylesheet list
        /// </summary>
        public bool CanStylesheetMoveUp => (!IsStylesheetSelected || Stylesheets.Count <= 1 || SelectedStylesheetIndex == 0)
                ? false
                : true;

        /// <summary>
        /// Indicates if an XSLT Stylesheet can be moved downwards in the stylesheet list
        /// </summary>
        public bool CanStylesheetMoveDown => (!IsStylesheetSelected || Stylesheets.Count <= 1 || SelectedStylesheetIndex == (Stylesheets.Count - 1))
                ? false
                : true;

        /// <summary>
        /// The currently selected parameter in the parameter list
        /// </summary>
        public object SelectedParameter { get; set; }

        /// <summary>
        /// Indicates if a parameter and no dummy parameter is selected in the parameter list so that it can be deleted
        /// </summary>
        public bool IsParameterSelected => (!IsStylesheetSelected || SelectedParameter == null || SelectedParameter.GetType().FullName == "MS.Internal.NamedObject")
                ? false
                : true;

        /// <summary>
        /// Indicates if an XSL Transformation can be applied
        /// </summary>
        public bool IsTransformable => (XmlInputPath == null || Stylesheets.Count == 0) ? false : true;

        /// <summary>
        /// Indicates if an XSL Transformation is running
        /// </summary>
        public bool TransformationIsRunning { get; set; } = false;

        /// <summary>
        /// Title of a message to be displayed in view
        /// </summary>
        public string MessageTitle { get; set; }

        /// <summary>
        /// Text of a message to be displayed in view
        /// </summary>
        public string MessageText { get; set; }

        /// <summary>
        /// Icon of a message to be displayed in view
        /// </summary>
        public MessageIcon DisplayMessageIcon { get; set; } = MessageIcon.No;

        /// <summary>
        /// Indicates if a modal message shall be displayed in view
        /// </summary>
        public bool ShowMessage { get; set; } = false;
        
        /// <summary>
        /// Indicates if a awaitable async message shall be displayed in view
        /// Raises event if message has been displayed
        /// </summary>
        public bool ShowAsyncMessage
        {
            get => mShowAsyncMessage;
            set
            {
                mShowAsyncMessage = value;
                if (!value)
                    asyncMessageShown.Set();
            }
        }

        /// <summary>
        /// List of xml-stylesheet declarations derived from XML input file
        /// </summary>
        /// <remarks>
        /// Object for binding data to display and return values in view
        /// </remarks>
        public ObservableCollection<XmlStylesheet> XmlStylesheetDeclarations { get; set; } = new ObservableCollection<XmlStylesheet>();

        /// <summary>
        /// Indicates if the awaitable async custom xml-stylesheet dialog shall be displayed in view
        /// Raises event if dialog has been displayed
        /// </summary>
        public bool ShowAsyncXmlStylesheetDialog
        {
            get => mShowAsyncXmlStylesheetDialog;
            set
            {
                mShowAsyncXmlStylesheetDialog = value;
                if (!value)
                    asyncXmlStylesheetDialogShown.Set();
            }
        }

        /// <summary>
        /// Event that gets invoked when XSLT Transformation starts
        /// </summary>
        public event EventHandler<EventArgs> TransformationStart;

        /// <summary>
        /// Event that gets invoked when XSLT Transformation starts
        /// </summary>
        public event EventHandler<EventArgs> TransformationEnd;

        /*
        /// <summary>
        /// Proposal for the output directory for the XSLT Transformation result file
        /// </summary>
        public string OutputDirectoryProposal { get; set; }
        */

        /// <summary>
        /// Proposal for the output path of the XSLT Transformation result file,
        /// triggers save file dialog in view
        /// </summary>
        public string OutputFilePathProposal { get; set; }

        /// <summary>
        /// Path of the XSLT Transformation result file,
        /// gets set from view (save file dialog)
        /// </summary>
        public string OutputFilePath
        {
            get => mOutputFilePath;
            set
            {
                mOutputFilePath = value;
                // Don't trigger outputFileSelected when OutputFilePath is reset
                if (value != null) // if save file dialog is cancelled String.Empty is returned so it still triggers
                    outputFileSelected.Set();
            }
        }

        #endregion

        #region Public Commands

        /// <summary>
        /// The command to remove the selected XSLT Stylesheet from stylesheet list
        /// </summary>
        public ICommand RemoveStylesheetCommand { get; set; }

        /// <summary>
        /// The command to move the selected XSLT Stylesheet upwards in stylesheet list
        /// </summary>
        public ICommand MoveStylesheetUpCommand { get; set; }

        /// <summary>
        /// The command to move the selected XSLT Stylesheet downwards in stylesheet list
        /// </summary>
        public ICommand MoveStylesheetDownCommand { get; set; }

        /// <summary>
        /// The command to add or insert a parameter to parameter list
        /// </summary>
        public ICommand AddParameterCommand { get; set; }

        /// <summary>
        /// The command to remove the selected parameter from parameter list
        /// </summary>
        public ICommand RemoveParameterCommand { get; set; }

        /// <summary>
        /// The command to transform the input XML with the stylesheets
        /// </summary>
        public ICommand TransformCommand { get; set; }

        /// <summary>
        /// The command to sync the settings from the persistent configuration
        /// </summary>
        public ICommand SyncSettingsFromConfigurationCommand { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public MainViewModel(IReadSettings settings)
        {
            mSettings = settings;
            RemoveStylesheetCommand = new RelayCommand(RemoveStylesheet);
            MoveStylesheetUpCommand = new RelayCommand(MoveStylesheetUp);
            MoveStylesheetDownCommand = new RelayCommand(MoveStylesheetDown);
            AddParameterCommand = new RelayCommand(AddParameter);
            RemoveParameterCommand = new RelayCommand(RemoveParameter);
            TransformCommand = new RelayCommand(Transform);
            SyncSettingsFromConfigurationCommand = new RelayCommand(SyncSettingsFromConfiguration);
        }

        #endregion

        #region Input Helper Methods

        /// <summary>
        /// Checks if file exists, if XML is wellformed and potentially sets XmlInputPath
        /// </summary>
        /// <param name="inputPath">path to input XML file</param>
        private async void CheckAndSetXmlInputPath(string inputPath)
        {
            // check if XML file can be loaded
            string checkPath = await XmlInput<string>(inputPath);

            if (!String.IsNullOrEmpty(checkPath))
                XmlInputPath = checkPath;
        }

        /// <summary>
        /// Checks for each file in inputFileList if it can be processed as XSLT stylesheet
        /// and potentially adds it to XslStylesheets
        /// </summary>
        /// <param name="inputFileList">list of input files to be added to XSL Stylesheets list</param>
        private async Task CheckAndInputStylesheets(ObservableCollection<string> inputFileList)
        {
            // Create settings and resolver instances needed for stylesheet check
            XsltSettings xsltSettings = CreateXsltSettings();
            XmlUrlResolver stylesheetResolver = CreateXmlUrlResolver();

            // Process all items in the file list
            foreach (string inputPath in inputFileList)
            {
                // Check if the stylesheet can be loaded
                string checkPath = await CreateXslt<string>(inputPath, xsltSettings, stylesheetResolver);

                // if errors occured, process next item in file list
                if (String.IsNullOrEmpty(checkPath))
                    continue;

                // otherwise, create new item to put it into the stylesheet list
                XsltStylesheet newStylesheet = new XsltStylesheet() { Path = inputPath };
                
                // if a stylesheet is selected, insert the new stylesheet after that one
                if (IsStylesheetSelected)
                    Stylesheets.Insert(SelectedStylesheetIndex + 1, newStylesheet);
                // otherwise add it to the end of the stylesheet list
                else
                    Stylesheets.Add(newStylesheet);
                
                // select newly added stylesheet in stylesheet list
                SelectedStylesheet = newStylesheet;
            }
        }

        #endregion

        #region Command Helper Methods

        /// <summary>
        /// Removes selected XSLT Stylesheet from stylesheet list
        /// </summary>
        private void RemoveStylesheet()
        {
            if (!IsStylesheetSelected || TransformationIsRunning)
                return;

            // get stylesheets count
            int stylesheetsCount = Stylesheets.Count;

            // get current index of selected stylesheet
            int selectedStylesheetIndex = SelectedStylesheetIndex;

            // remove stylesheet at selected index from list
            Stylesheets.RemoveAt(selectedStylesheetIndex);

            // return if there are no more parameters in the list
            if (Stylesheets.Count == 0)
                return;

            // index of stylesheet to select
            int selectStylesheetIndex;

            // if there is only one parameter left in the list ...
            if (Stylesheets.Count == 1)
            {
                // ... set select parameter index to first parameter
                selectStylesheetIndex = 0;
            }
            // else if the selected parameter was the last ...
            else if (selectedStylesheetIndex == stylesheetsCount - 1)
            {
                // .. select previous parameter
                selectStylesheetIndex = selectedStylesheetIndex - 1;
            }
            else
            {
                // else select parameter at same position in the list
                selectStylesheetIndex = selectedStylesheetIndex;
            }

            // set parameter selection

            SelectedStylesheet = Stylesheets.ElementAt(selectStylesheetIndex);
        }

        /// <summary>
        /// Moves selected XSLT Stylesheet 1 position upwards in stylesheet list
        /// </summary>
        private void MoveStylesheetUp()
        {
            if (!CanStylesheetMoveUp || TransformationIsRunning)
                return;

            // move selected stylesheet one up
            SelectedStylesheetIndex--;
        }

        /// <summary>
        /// Moves selected XSLT Stylesheet 1 position downwards in stylesheet list
        /// </summary>
        private void MoveStylesheetDown()
        {
            if (!CanStylesheetMoveDown || TransformationIsRunning)
                return;

            // move selected stylesheet one down
            SelectedStylesheetIndex++;
        }

        /// <summary>
        /// Add new parameter to parameter list
        /// </summary>
        private void AddParameter()
        {
            if (!IsStylesheetSelected || TransformationIsRunning)
                return;

            // set newParameterIndex to parameters count
            int newParameterIndex = SelectedStylesheet.Parameters.Count;

            // set suffix for default new parameter name / value
            int suffix = newParameterIndex + 1;

            // create new parameter with default suffixed name / value
            StylesheetParameter newParameter = new StylesheetParameter()
            {
                Parent = SelectedStylesheet,
                Name = mStrings.GetString("AddedParameterNamePrefix") + suffix,
                Value = mStrings.GetString("AddedParameterValuePrefix") + suffix
            };

            // if no parameter or dummy parameter is selected ...
            if (!IsParameterSelected)
            {
                // ... add parameter at end of list
                SelectedStylesheet.Parameters.Add(newParameter);
            }
            else
            {
                // get index of selected parameter in list
                int selectedParameterIndex = SelectedStylesheet.Parameters.IndexOf((StylesheetParameter)SelectedParameter);

                // set newParameterIndex to after selected parameter in list
                newParameterIndex = selectedParameterIndex + 1;

                // insert new parameter after selected parameter in list
                SelectedStylesheet.Parameters.Insert(newParameterIndex, newParameter);
            }

            // set parameter selection to new parameter
            SelectedParameter = SelectedStylesheet.Parameters.ElementAt(newParameterIndex);
        }

        /// <summary>
        /// Removes selected parameter from parameter list
        /// </summary>
        private void RemoveParameter()
        {
            if (!IsStylesheetSelected || !IsParameterSelected || TransformationIsRunning)
                return;

            // get parameters count
            int parametersCount = SelectedStylesheet.Parameters.Count;

            // get index of selected parameter in list
            int selectedParameterIndex = SelectedStylesheet.Parameters.IndexOf((StylesheetParameter)SelectedParameter);

            // remove parameter from selected index in list
            SelectedStylesheet.Parameters.RemoveAt(selectedParameterIndex);

            // return if there are no more parameters in the list
            if (SelectedStylesheet.Parameters.Count == 0)
                return;

            // index of parameter to select
            int selectParameterIndex;

            // if there is only one parameter left in the list ...
            if (SelectedStylesheet.Parameters.Count == 1)
            {
                // ... set select parameter index to first parameter
                selectParameterIndex = 0;
            }
            // else if the selected parameter was the last ...
            else if (selectedParameterIndex == parametersCount - 1)
            {
                // .. select previous parameter
                selectParameterIndex = selectedParameterIndex - 1;
            }
            else
            {
                // else select parameter at same position in the list
                selectParameterIndex = selectedParameterIndex;
            }

            // set parameter selection
            SelectedParameter = SelectedStylesheet.Parameters.ElementAt(selectParameterIndex);
        }

        /// <summary>
        /// Applies the XSL Transformation(s)
        /// </summary>
        private async void Transform()
        {
            TransformationStart.Invoke(null, EventArgs.Empty);
            await RunCommand(() => TransformationIsRunning, async () =>
            {
                // Create XmlReaderSettings according to current XslTransformerSettings
                XmlReaderSettings readerSettings = CreateXmlReaderSettings();

                // Get XmlReader instance for XML input file
                XmlReader inputReader = await XmlInput<XmlReader>(XmlInputPath, readerSettings);
                // Return if there were errors
                if (inputReader == null)
                    return;

                // Create settings and resolver instances needed for stylesheet(s)
                XsltSettings xsltSettings = CreateXsltSettings();
                XmlUrlResolver stylesheetResolver = CreateXmlUrlResolver();

                // Create argument list for XSLT parameters
                XsltArgumentList arguments = new XsltArgumentList();

                // initialize XslCompiledTransform instance
                XslCompiledTransform xslt = null;

                // Create memory stream for transformation output
                MemoryStream outputStream = new MemoryStream();

                // Create memory stream for transformation input
                MemoryStream inputStream = new MemoryStream();

                // Initialize path variable to remember last processed stylesheet
                string lastProcessedStylesheet = String.Empty;

                // iterate over stylesheet list
                bool first = true;
                foreach (XsltStylesheet stylesheet in Stylesheets)
                {
                    // create xslt instance
                    xslt = await CreateXslt<XslCompiledTransform>(stylesheet.Path, xsltSettings, stylesheetResolver);

                    // Return if errors occured
                    if (xslt == null)
                        return;

                    // empty out xslt arguments
                    arguments.Clear();

                    // iterate over parameter list
                    foreach (StylesheetParameter parameter in stylesheet.Parameters)
                    {
                        // add parameter to argument list
                        arguments.AddParam(parameter.Name, String.Empty, parameter.Value);
                    }

                    // input stream is used if there are more than one stylesheets in the list
                    if (!first)
                    {
                        // dispose and recreate input stream
                        inputStream.Close();
                        inputStream = new MemoryStream();

                        // copy output stream to input stream
                        outputStream.WriteTo(inputStream);

                        // dispose and recreate output stream
                        outputStream.Close();
                        outputStream = new MemoryStream();

                        // reset input stream position to read from the beginning
                        inputStream.Seek(0, SeekOrigin.Begin);

                        // recreate input reader from input stream with settings
                        inputReader = XmlReader.Create(inputStream, readerSettings);

                        // try to parse the xml in stream
                        try
                        {
                            while (await inputReader.ReadAsync()) ;
                        }
                        catch (Exception e)
                        {
                            // if errors occured show error message and return
                            await AsyncMessage(mStrings.GetString("TransformationResultErrorMsgTitle"), string.Format(mStrings.GetString("TransformationResultErrorMsgText"), lastProcessedStylesheet, e.Message), MessageIcon.Error);
                            inputStream.Close();
                            outputStream.Close();
                            return;
                        }

                        // reset input stream position to read from the beginning
                       //inputStream.Seek(0, SeekOrigin.Begin);
                    }

                    // Try to apply the transformation, put result to output stream
                    try
                    {
                        xslt.Transform(inputReader, arguments, outputStream);
                    }
                    catch (Exception e)
                    {
                        // if errors occured show error message and return
                        await AsyncMessage(mStrings.GetString("XslTransformationErrorMsgTitle"), string.Format(mStrings.GetString("XslTransformationErrorMsgText"), stylesheet.Path, e.Message), MessageIcon.Error);
                        inputStream.Close();
                        outputStream.Close();
                        return;
                    }

                    // remember last processed stylesheets for possible error messages
                    lastProcessedStylesheet = stylesheet.Path;

                    // reset output stream position to read from the beginning
                    outputStream.Seek(0, SeekOrigin.Begin);

                    if (first)
                        first = false;
                }

                // close input stream as it is no longer needed
                inputStream.Close();

                // Propose input directory as output directory, set property that is bound to view
                string outputDirectoryProposal = Path.GetDirectoryName(XmlInputPath);

                // Get xml input file name without extension
                string inputFileNameWithoutExtension = Path.GetFileNameWithoutExtension(XmlInputPath);

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

                // Concatenate output file name proposal
                string outputFileNameProposal = inputFileNameWithoutExtension + ".out." + extensionProposal;

                // Set combined OutputFilePathProposal property, triggering save file dialog in view
                OutputFilePathProposal = Path.Combine(outputDirectoryProposal, outputFileNameProposal);

                // wait until the output file is selected
                await Task.Run(() =>
                {
                    outputFileSelected.WaitOne();
                });

                // Return if the user aborted or errors occured (OutputFilePath is set to String.Empty)
                if (OutputFilePath == String.Empty)
                {
                    outputStream.Close();
                    OutputFilePath = null;
                    OutputFilePathProposal = null;
                    return;
                }
                    

                // Try to save output to OutputFilePath
                try
                {
                    using (FileStream fileStream = File.Open(OutputFilePath, FileMode.Create, FileAccess.Write))
                    {
                        // If encoding is UTF8 skip byte order mark if set so
                        if (xslt.OutputSettings.Encoding == Encoding.UTF8 && !mSettings.GetValue<bool>(Setting.WriteUtf8Bom))
                        {
                            // Get standard utf8 bom-bytes
                            byte[] bom = Encoding.UTF8.GetPreamble();

                            // Check if outputStream starts with bom
                            for (int i = 0; i < bom.Length; i++)
                            {
                                // outputStream.ReadByte() moves position
                                byte testByte = Convert.ToByte(outputStream.ReadByte());
                                
                                // If bytes don't match ...
                                if (testByte != bom[i])
                                {
                                    // ... reset position to 0 and break loop
                                    outputStream.Seek(0, SeekOrigin.Begin);
                                    break;
                                }
                            }
                        }
                        // stream.CopyTo() copies from current stream position until the end
                        outputStream.CopyTo(fileStream);
                    }
                }
                catch(Exception e)
                {
                    await AsyncMessage(mStrings.GetString("OutputFileErrorMsgTitle"), string.Format(mStrings.GetString("OutputFileErrorMsgText"), OutputFilePath, e.Message), MessageIcon.Error);
                    return;
                }
                finally
                {
                    // be sure the output stream is disposed
                    outputStream.Close();
                }

                // Reset OutputFilePath so it can trigger outputFileSelected again
                OutputFilePath = null;

                // Reset OutputFilePathProposal so it can trigger save file dialog again
                OutputFilePathProposal = null;

                // Success message
                await AsyncMessage(mStrings.GetString("TransformationSuccessMsgTitle"), mStrings.GetString("TransformationSuccessMsgText"), MessageIcon.Success);
            });
            TransformationEnd.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Syncs the settings from the persistent configuration
        /// </summary>
        private void SyncSettingsFromConfiguration()
        {
            mSettings.SyncFromConfiguration();
        }

        #endregion

        #region XML Helper Methods

        /// <summary>
        /// Gets XmlInput as file path string or new XmlReader instance
        /// with XmlReaderSettings according to current XslTransformerSettings
        /// </summary>
        /// <typeparam name="T">string for file path or XmlReader for reader instance</typeparam>
        /// <param name="inputPath">Path to XML input file</param>
        /// <returns>filePath string / XmlReader instance or null if errors occured</returns>
        private async Task<T> XmlInput<T>(string inputPath)
        {
            // Create XmlReaderSettings according to current XslTransformerSettings
            XmlReaderSettings settings = CreateXmlReaderSettings();

            return await XmlInput<T>(inputPath, settings);
        }

        /// <summary>
        /// Gets XmlInput as file path string or new XmlReader instance
        /// </summary>
        /// <typeparam name="T">string for file path or XmlReader for reader instance</typeparam>
        /// <param name="inputPath">Path to XML input file</param>
        /// <param name="settings">The XmlReaderSettings instance to process</param>
        /// <returns>filePath string / XmlReader instance or null if errors occured</returns>
        private async Task<T> XmlInput<T>(string inputPath, XmlReaderSettings settings)
        {
            if (!File.Exists(inputPath))
            {
                await AsyncMessage(mStrings.GetString("FileNotFoundMsgTitle"), string.Format(mStrings.GetString("FileNotFoundMsgText"), inputPath), MessageIcon.Error);
                return default(T);
            }

            // Create the XmlReader object
            XmlReader reader = XmlReader.Create(inputPath, settings);

            // Empty list for xml-stylesheet declarations
            XmlStylesheetDeclarations.Clear();

            // Search for xml-stylesheet declarations after XML input file choice by user (Type is string for path)
            bool searchXmlStylesheet = (typeof(T).Equals(typeof(string))) ? true : false;

            // Try to parse the file and pick xml-stylesheet declarations
            try
            {
                while (await reader.ReadAsync())
                {
                    // search for xml-stylesheet declarations if set so
                    if (!searchXmlStylesheet)
                        continue;
                    // stop searching when root element is reached
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
                    XmlStylesheetDeclarations.Add(xmlStylesheetItem);
                }
            }
            catch (XmlException e)
            {
                await AsyncMessage(mStrings.GetString("XmlInputFileErrorMsgTitle"), string.Format(mStrings.GetString("XmlInputFileErrorMsgText"), inputPath, e.Message), MessageIcon.Error);
                return default(T);
            }
            catch (XmlSchemaValidationException e)
            {
                await AsyncMessage(mStrings.GetString("XmlInputFileValidationErrorMsgTitle"), string.Format(mStrings.GetString("XmlInputFileValidationErrorMsgText"), inputPath, e.Message), MessageIcon.Error);
                return default(T);
            }
            catch (Exception e)
            {
                await AsyncMessage(mStrings.GetString("XmlInputFileErrorMsgTitle"), string.Format(mStrings.GetString("XmlInputFileErrorMsgText"), inputPath, e.Message), MessageIcon.Error);
                return default(T);
            }

            // if there are XmlStylesheet items in the declarations list show custom dialog
            if (XmlStylesheetDeclarations.Any())
                await XmlStylesheetDialog();

            // create input file list
            ObservableCollection<string> inputFileList = new ObservableCollection<string>();

            // get input directory
            string inputDirectory = Path.GetDirectoryName(inputPath);

            // add selected xml-stylesheets href to input file list (all deselected if user chose No)
            foreach (XmlStylesheet xs in XmlStylesheetDeclarations)
            {
                if (xs.IsSelected)
                {
                    // combine input directory and href attribute, if href is absolute Path.Combine will only return that
                    inputFileList.Add(Path.Combine(inputDirectory, xs.Href));
                }
            }

            // if there are XSLT files to input
            if (inputFileList.Any())
            {
                // empty stylesheets list
                Stylesheets.Clear();

                // try to add input file list to stylesheets
                await CheckAndInputStylesheets(inputFileList);
            }

            Type type = typeof(T);
            object returnValue = default(T);
            if (type.Equals(typeof(string)))
                returnValue = inputPath;
            else
                returnValue = reader;
            return (T)returnValue;
        }

        /// <summary>
        /// Create XmlReaderSettings according to current XslTransformerSettings
        /// </summary>
        /// <returns>a new XmlReaderSettings</returns>
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

        // ValidationEventHandler callback (strangely enough,message overlay seems stuck for seconds before message displays)
        private async void ValidationCallBack(object sender, ValidationEventArgs e)
        {
            // If a ValidationEventHandler is registered also ValidationErrors don't throw exceptions
            // so a distinction is necessary here
            if (e.Severity == XmlSeverityType.Warning)
                await AsyncMessage(mStrings.GetString("XmlValidationWarningMsgTitle"), string.Format(mStrings.GetString("XmlValidationMsgText"), e.Message), MessageIcon.Warning);
            else
                await AsyncMessage(mStrings.GetString("XmlValidationErrorMsgTitle"), string.Format(mStrings.GetString("XmlValidationMsgText"), e.Message), MessageIcon.Warning);
        }

        /// <summary>
        /// Create XsltSettings according to current XslTransformerSettings
        /// </summary>
        /// <returns>a new XsltSettings</returns>
        private XsltSettings CreateXsltSettings()
        {
            XsltSettings xsltSettings = new XsltSettings(
                mSettings.GetValue<bool>(Setting.EnableDocumentFunction),
                mSettings.GetValue<bool>(Setting.EnableScript));
            return xsltSettings;
        }

        /// <summary>
        /// Create a xml url resolver with default credentials
        /// </summary>
        /// <returns>a new default XmlUrlResolver</returns>
        private XmlUrlResolver CreateXmlUrlResolver()
        {
            XmlUrlResolver resolver = new XmlUrlResolver();
            resolver.Credentials = CredentialCache.DefaultCredentials;
            return resolver;
        }

        /// <summary>
        /// Creates XsltCompiledTransform to check and return inputPath stylesheet
        /// or to return the new XsltCompiledTransform instance
        /// </summary>
        /// <typeparam name="T">string for file path or XsltCompiledTransform for xslt instance</typeparam>
        /// <param name="inputPath">Path to XSLT stylesheet file</param>
        /// <param name="xsltSettings">settings for the creation of XsltCompiledTransform</param>
        /// <param name="stylesheetResolver">xml url resolver to process </param>
        /// <returns>filePath string / XsltCompiledTransform instance or null if errors occured</returns>
        private async Task<T> CreateXslt<T>(string inputPath, XsltSettings xsltSettings, XmlUrlResolver stylesheetResolver)
        {
            if (!File.Exists(inputPath))
            {
                await AsyncMessage(mStrings.GetString("FileNotFoundMsgTitle"), string.Format(mStrings.GetString("FileNotFoundMsgText"), inputPath), MessageIcon.Error);
                return default(T);
            }

            try
            {
                // Create a XslCompiledTransform instance and load the stylesheet
                XslCompiledTransform xslt = new XslCompiledTransform();
                xslt.Load(inputPath, xsltSettings, stylesheetResolver);

                Type type = typeof(T);
                object returnValue = default(T);
                if (type.Equals(typeof(string)))
                    returnValue = inputPath;
                else
                    returnValue = xslt;
                return (T)returnValue;
            }
            catch (XsltException e)
            {
                await AsyncMessage(mStrings.GetString("XsltErrorMsgTitle"), string.Format(mStrings.GetString("XsltXsltErrorMsgText"), inputPath, e.Message), MessageIcon.Error);
                return default(T);
            }
            catch (XmlException e)
            {
                await AsyncMessage(mStrings.GetString("XsltErrorMsgTitle"), string.Format(mStrings.GetString("XsltXmlErrorMsgText"), inputPath, e.Message), MessageIcon.Error);
                return default(T);
            }
            catch (Exception e)
            {
                await AsyncMessage(mStrings.GetString("XsltErrorMsgTitle"), string.Format(mStrings.GetString("XsltXsltErrorMsgText"), inputPath, e.Message), MessageIcon.Error);
                return default(T);
            }
        }

        #endregion

        #region Message Helper Methods

        /// <summary>
        /// Show modal error message in view
        /// </summary>
        /// <param name="title">The title of the error message to display</param>
        /// <param name="message">The message content.</param>
        private void Message(string title, string message, MessageIcon icon = MessageIcon.No)
        {
            MessageTitle = title;
            MessageText = message;
            DisplayMessageIcon = icon;

            // ShowMessage triggers the modal message overlay in MessageDialog UserControl
            ShowMessage = true;

            // Is reset here because it does not work from UserControl class after its ShowModalMessageExternal method
            // maybe modal message overlay (kind of new window) is blocking main ui thread
            ShowMessage = false;
        }

        /// <summary>
        /// Show awaitable async error message in view
        /// </summary>
        /// <param name="title">The title of the error message to display</param>
        /// <param name="message">The message content</param>
        /// <param name="icon">The message icon</param>
        /// <returns>awaitable Task</returns>
        private async Task AsyncMessage(string title, string message, MessageIcon icon = MessageIcon.No)
        {
            MessageTitle = title;
            MessageText = message;
            DisplayMessageIcon = icon;

            // ShowAsyncMessage triggers the message overlay managed in MessageDialog UserControl
            ShowAsyncMessage = true;

            // wait until the message is shown and clicked away
            await Task.Run(() =>
            {
                asyncMessageShown.WaitOne();
            });
        }

        #endregion

        #region Custom Dialog Helper Methods

        /// <summary>
        /// Show modal xml-stylesheet dialog in view
        /// </summary>
        /// <returns>awaitable Task</returns>
        private async Task XmlStylesheetDialog()
        {
            // ShowAsyncXmlStylesheetDialog triggers the dialog overlay managed in XmlStylesheetDialog UserControl
            ShowAsyncXmlStylesheetDialog = true;

            // wait until user interaction with dialog is finished
            await Task.Run(() =>
            {
                asyncXmlStylesheetDialogShown.WaitOne();
            });
        }

        #endregion
    }
}

// DLL blocked ? |win| + |r| > taskkill /im msbuild.exe /f /t
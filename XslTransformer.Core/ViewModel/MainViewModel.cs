using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using PropertyChanged;
using System.Threading;
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
        /// Holds DI reference to XmlProcessor
        /// </summary>
        private readonly IProcessXml mXmlProcessor;

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
        public string XmlInputPath {
            get => Data.XmlInputPath;
            private set => Data.XmlInputPath = value;
        }

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
        public ObservableCollection<XsltStylesheet> Stylesheets
        {
            get => Data.Stylesheets;
            private set => Data.Stylesheets = value;
        }

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

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="settings">readable Settings dependency injection</param>
        /// <param name="xmlProcessor">xml processor dependency injection</param>
        public MainViewModel(IReadSettings settings, IProcessXml xmlProcessor)
        {
            mSettings = settings;
            mXmlProcessor = xmlProcessor;
            mXmlProcessor.ShowAsyncMessage += XmlProcessor_ShowAsyncMessage;
            RemoveStylesheetCommand = new RelayCommand(RemoveStylesheet);
            MoveStylesheetUpCommand = new RelayCommand(MoveStylesheetUp);
            MoveStylesheetDownCommand = new RelayCommand(MoveStylesheetDown);
            AddParameterCommand = new RelayCommand(AddParameter);
            RemoveParameterCommand = new RelayCommand(RemoveParameter);
            TransformCommand = new RelayCommand(Transform);
        }

        #endregion

        #region Input Helper Methods

        /// <summary>
        /// Checks if file exists, checks xml and potentially sets XmlInputPath and Stylesheets from XmlStylesheetDeclarations
        /// </summary>
        /// <param name="inputPath">path to input XML file</param>
        private async void CheckAndSetXmlInputPath(string inputPath)
        {
            // check if XML file can be loaded
            ObservableCollection<XmlStylesheet> xmlStylesheetDeclarations = await mXmlProcessor.CheckXmlFile(inputPath);

            // return on error
            if (xmlStylesheetDeclarations == null) return;

            // set xml input file path
            XmlInputPath = inputPath;

            // set xml-stylesheet declarations list
            XmlStylesheetDeclarations = xmlStylesheetDeclarations;

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
        }

        /// <summary>
        /// Checks for each file in inputFileList if it can be processed as XSLT stylesheet
        /// and potentially adds it to XslStylesheets
        /// </summary>
        /// <param name="inputFileList">list of input files to be added to XSL Stylesheets list</param>
        private async Task CheckAndInputStylesheets(ObservableCollection<string> stylesheetFileList)
        {
            // Process all items in the stylesheet file list
            foreach (string stylesheetFilePath in stylesheetFileList)
            {
                // check stylesheet
                bool stylesheetOkay = await mXmlProcessor.CheckStylesheet(stylesheetFilePath);

                // process next item in stylesheet list on error
                if (!stylesheetOkay) continue;

                // otherwise, create new item to put it into the stylesheet list
                XsltStylesheet newStylesheet = new XsltStylesheet() { Path = stylesheetFilePath };
                
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
                // Perform xsl transformation according to current Data, get proposal for output file name
                string outputFileNameProposal = await mXmlProcessor.Transform();

                // return on error
                if (outputFileNameProposal == null)
                {
                    mXmlProcessor.OutputStream.Close();
                    return;
                }
                
                // Propose input directory as output directory
                string outputDirectoryProposal = Path.GetDirectoryName(XmlInputPath);

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
                    mXmlProcessor.OutputStream.Close();
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
                        if (mXmlProcessor.OutputEncoding == Encoding.UTF8 && !mSettings.GetValue<bool>(Setting.WriteUtf8Bom))
                        {
                            // Get standard utf8 bom-bytes
                            byte[] bom = Encoding.UTF8.GetPreamble();

                            // Check if outputStream starts with bom
                            for (int i = 0; i < bom.Length; i++)
                            {
                                // stream.ReadByte() moves position
                                byte testByte = Convert.ToByte(mXmlProcessor.OutputStream.ReadByte());
                                
                                // If bytes don't match ...
                                if (testByte != bom[i])
                                {
                                    // ... reset position to 0 and break loop
                                    mXmlProcessor.OutputStream.Seek(0, SeekOrigin.Begin);
                                    break;
                                }
                            }
                        }
                        // stream.CopyTo() copies from current stream position until the end
                        mXmlProcessor.OutputStream.CopyTo(fileStream);
                    }
                }
                catch(Exception e)
                {
                    await MessageAsync(MessageType.OutputFileError, new object[] { OutputFilePath, e.Message });
                    return;
                }
                finally
                {
                    // be sure the output stream is disposed
                    mXmlProcessor.OutputStream.Close();
                }

                // Reset OutputFilePath so it can trigger outputFileSelected again
                OutputFilePath = null;

                // Reset OutputFilePathProposal so it can trigger save file dialog again
                OutputFilePathProposal = null;

                // Success message
                await MessageAsync(MessageType.TransformationSuccess);
            });
            TransformationEnd.Invoke(null, EventArgs.Empty);
        }

        #endregion

        #region Message Helper Methods

        /// <summary>
        /// Callback method to display async messages from XmlProcessor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">the message event arguments</param>
        private async void XmlProcessor_ShowAsyncMessage(object sender, MessageEventArgs e)
        {
            // show message
            await MessageAsync(e.MessageType, e.MessageParams);
            // fire auto reset event
            mXmlProcessor.AsyncMessageShown.Set();
        }

        /// <summary>
        /// Shows awaitable async message in view
        /// </summary>
        /// <param name="messageType">The type of message to be displayed</param>
        /// <param name="messageParams">String representations of these objects will be included in message</param>
        /// <returns>awaitable Task</returns>
        private async Task MessageAsync(MessageType messageType, object[] messageParams = null)
        {
            // set icon by message type
            MessageIcon icon;
            switch (messageType)
            {
                case MessageType.TransformationSuccess:
                    icon = MessageIcon.Success;
                    break;
                case MessageType.XmlValidationWarning:
                    icon = MessageIcon.Warning;
                    break;
                default:
                    icon = MessageIcon.Error;
                    break;
            }

            // get title by message type
            string title = mStrings.GetString(messageType.ToString() + "MsgTitle");

            // get text string by message type
            string text = mStrings.GetString(messageType.ToString() + "MsgText");

            // format text string by message params if present
            string message;
            if (messageParams == null)
                message = text;
            else
                message = String.Format(text, messageParams);

            // Set message properties that are bound to view (MessageDialog UserControl)

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
// Seems to be fixed with Visual Studio Version 15.3.2
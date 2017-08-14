using System;
using System.Linq;
using System.Windows.Input;
using System.Collections.Generic;

namespace XslTransformer.Core
{
    /// <summary>
    /// View model for settings page
    /// </summary>
    public class SettingsViewModel : BaseViewModel
    {
        #region Private Members

        /// <summary>
        /// Holds DI reference to Settings
        /// </summary>
        private readonly IReadWriteSettings mSettings;

        #endregion

        #region Public Properties

        /// <summary>
        /// The setting indicating whether to enable support for the XSLT document() function
        /// </summary>
        public bool EnableDocument​Function
        {
            get => mSettings.GetValue<bool>(Setting.EnableDocumentFunction);
            set => mSettings.Set(Setting.EnableDocumentFunction, value);
        }

        /// <summary>
        /// The setting indicating whether to enable support for embedded script blocks
        /// </summary>
        public bool Enable​Script
        {
            get => mSettings.GetValue<bool>(Setting.EnableScript);
            set => mSettings.Set(Setting.EnableScript, value);
        }

        /// <summary>
        /// The setting indicating whether to allow/ignore the processing of DTDs by <see cref="System.Xml.XmlReader"/>
        /// </summary>
        public XmlReaderDtdProcessing DtdProcessing
        {
            get => mSettings.GetValue<XmlReaderDtdProcessing>(Setting.DtdProcessing);
            set => mSettings.Set(Setting.Dtd​Processing, value);
        }

        /// <summary>
        /// XmlReaderDtdProcessing enum values for combobox binding
        /// </summary>
        public IEnumerable<XmlReaderDtdProcessing> DtdProcessingValues => Enum.GetValues(typeof(XmlReaderDtdProcessing)).Cast<XmlReaderDtdProcessing>();

        /// <summary>
        /// The setting indicating whether the reader should validate data, and what type of validation to perform (DTD or schema)
        /// </summary>
        public XmlReaderValidationType ValidationType
        {
            get => mSettings.GetValue<XmlReaderValidationType>(Setting.ValidationType);
            set => mSettings.Set(Setting.ValidationType, value);
        }

        /// <summary>
        /// XmlReaderValidationType enum values for combobox binding
        /// </summary>
        public IEnumerable<XmlReaderValidationType> ValidationTypeValues => Enum.GetValues(typeof(XmlReaderValidationType)).Cast<XmlReaderValidationType>();

        /// <summary>
        /// The setting indicating whether character and name checking is enabled (will be set to true either way)
        /// </summary>
        public bool CheckCharacters
        {
            get => mSettings.GetValue<bool>(Setting.CheckCharacters);
            set => mSettings.Set(Setting.CheckCharacters, value);
        }

        /// <summary>
        /// The setting indicating whether to allow XML attributes (xml:*) in instance documents even when they're not defined in the schema
        /// </summary>
        public bool XsdValidationFlag_AllowXmlAttributes
        {
            get => mSettings.GetValue<bool>(Setting.XsdValidationFlag_AllowXmlAttributes);
            set => mSettings.Set(Setting.XsdValidationFlag_AllowXmlAttributes, value);
        }

        /// <summary>
        /// The setting indicating whether to process identity constraints (xs:ID, xs:IDREF, xs:key, xs:keyref, xs:unique) encountered during validation
        /// </summary>
        public bool XsdValidationFlag_ProcessIdentityConstraints
        {
            get => mSettings.GetValue<bool>(Setting.XsdValidationFlag_ProcessIdentityConstraints);
            set => mSettings.Set(Setting.XsdValidationFlag_ProcessIdentityConstraints, value);
        }

        /// <summary>
        /// The setting indicating whether to process schemas specified by the xsi:schemaLocation or xsi:noNamespaceSchemaLocation attribute
        /// </summary>
        public bool XsdValidationFlag_ProcessSchemaLocation
        {
            get => mSettings.GetValue<bool>(Setting.XsdValidationFlag_ProcessSchemaLocation);
            set => mSettings.Set(Setting.XsdValidationFlag_ProcessSchemaLocation, value);
        }

        /// <summary>
        /// The setting indicating whether to process inline XML Schemas during validation
        /// </summary>
        public bool XsdValidationFlag_ProcessInlineSchema
        {
            get => mSettings.GetValue<bool>(Setting.XsdValidationFlag_ProcessInlineSchema);
            set => mSettings.Set(Setting.XsdValidationFlag_ProcessInlineSchema, value);
        }

        /// <summary>
        /// The setting indicating whether to report validation warnings.
        /// A warning is typically issued when there is no DTD or XML Schema to validate a particular element or attribute against.
        /// </summary>
        public bool XsdValidationFlag_ReportValidationWarnings
        {
            get => mSettings.GetValue<bool>(Setting.XsdValidationFlag_ReportValidationWarnings);
            set => mSettings.Set(Setting.XsdValidationFlag_ReportValidationWarnings, value);
        }

        /// <summary>
        /// The setting indicating whether character and name checking is enabled (will be set to true either way)
        /// </summary>
        public bool WriteUtf8Bom
        {
            get => mSettings.GetValue<bool>(Setting.WriteUtf8Bom);
            set => mSettings.Set(Setting.WriteUtf8Bom, value);
        }

        /// <summary>
        /// event that gets invoked at the end of SettingsOkay and SettingsCancel helper methods 
        /// </summary>
        public event EventHandler<EventArgs> BackToMainPage;

        #endregion

        #region Public Commands

        /// <summary>
        /// The command to save settings and return to the main page
        /// </summary>
        public ICommand SettingsOkayCommand { get; set; }

        /// <summary>
        /// The command to cancel changes to settings and return to the main page
        /// </summary>
        public ICommand SettingsCancelCommand { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public SettingsViewModel(IReadWriteSettings settings)
        {
            mSettings = settings;
            SettingsOkayCommand = new RelayCommand(SettingsOkay);
            SettingsCancelCommand = new RelayCommand(SettingsCancel);
        }

        #endregion

        #region Command Helper Methods

        private void SettingsOkay()
        {
            mSettings.SaveToConfiguration();
            BackToMainPage?.Invoke(null, EventArgs.Empty);
        }

        private void SettingsCancel()
        {
            BackToMainPage?.Invoke(null, EventArgs.Empty);
        }

        #endregion
    }
}

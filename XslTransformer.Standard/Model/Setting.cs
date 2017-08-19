namespace XslTransformer.Core
{
    public enum Setting
    {
        /// <summary>
        /// Has a XmlReaderDtdProcessing enum value (default: XmlReaderDtdProcessing.Prohibit)
        /// </summary>
        /// <remarks>
        /// Indicating if DTDs are prohibited, ignored or parsed
        /// </remarks>
        Dtd​Processing,

        /// <summary>
        /// Has a XmlReaderValidationType enum value (default: XmlReaderValidationType.None)
        /// </summary>
        /// <remarks>
        /// Indicating whether the reader should validate data, and what type of validation to perform (DTD or schema)
        /// </remarks>
        ValidationType,

        /// <summary>
        /// Has a boolean value (default: true)
        /// </summary>
        /// <remarks>
        /// Indicating whether character and name checking is enabled
        /// </remarks>
        CheckCharacters,

        /// <summary>
        /// Has a boolean value (default: false)
        /// </summary>
        /// <remarks>
        /// Indicating whether to allow XML attributes (xml:*) in instance documents even when they're not defined in the schema
        /// </remarks>
        XsdValidationFlag_AllowXmlAttributes,

        /// <summary>
        /// Has a boolean value (default: true)
        /// </summary>
        /// <remarks>
        /// Indicating whether to process identity constraints (xs:ID, xs:IDREF, xs:key, xs:keyref, xs:unique) encountered during validation
        /// </remarks>
        XsdValidationFlag_ProcessIdentityConstraints,

        /// <summary>
        /// Has a boolean value (default: true)
        /// </summary>
        /// <remarks>
        /// Indicating whether to process schemas specified by the xsi:schemaLocation or xsi:noNamespaceSchemaLocation attribute
        /// </remarks>
        XsdValidationFlag_ProcessSchemaLocation,

        /// <summary>
        /// Has a boolean value (default: false)
        /// </summary>
        /// <remarks>
        /// Indicating whether to process inline XML Schemas during validation
        /// </remarks>
        XsdValidationFlag_ProcessInlineSchema,

        /// <summary>
        /// Has a boolean value (default: false)
        /// </summary>
        /// <remarks>
        /// Indicating whether to report validation warnings.
        /// A warning is typically issued when there is no DTD or XML Schema to validate a particular element or attribute against.
        /// </remarks>
        XsdValidationFlag_ReportValidationWarnings,

        /// <summary>
        /// Has a boolean value (default: false)
        /// </summary>
        /// <remarks>
        /// Indicating whether to enable support for the XSLT document() function
        /// </remarks>
        EnableDocumentFunction,

        /// <summary>
        /// Has a boolean value (default: false)
        /// </summary>
        /// <remarks>
        /// Indicating whether to enable support for embedded script blocks
        /// </remarks>
        EnableScript,

        /// <summary>
        /// Has a boolean value (default: true)
        /// </summary>
        /// <remarks>
        /// Indicating wether to write UTF8 byte order mark
        /// </remarks>
        WriteUtf8Bom
    }
}

namespace XslTransformer.Core
{
    public enum XmlReaderValidationType
    {
        /// <summary>
        /// The XmlReader does not validate data, or perform any type assignment.
        /// </summary>
        None,

        /// <summary>
        /// Validation is performed using a document type definition (DTD).
        /// The DtdProcessing property must also be set to Parse.
        /// </summary>
        DTD,

        /// <summary>
        /// Validation and type assignment is performed using an XML Schema definition language (XSD) schema.
        /// The ProcessSchemaLocation option or / and the ProcessInlineSchema option must also be enabled.
        /// </summary>
        Schema
    }
}

namespace XslTransformer.Core
{
    public enum XmlReaderDtdProcessing
    {
        /// <summary>
        /// XmlReader throws an XmlException exception when a DTD is encountered.
        /// </summary>
        Prohibit,

        /// <summary>
        /// DTD processing is disabled without warnings or exceptions.
        /// </summary>
        Ignore,

        /// <summary>
        /// Enable DTD processing.
        /// </summary>
        Parse
    }
}

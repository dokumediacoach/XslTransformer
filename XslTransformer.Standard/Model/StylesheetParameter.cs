using System;
using System.ComponentModel;
using System.Reflection;
using System.Resources;

namespace XslTransformer.Core
{
    public class StylesheetParameter : IDataErrorInfo
    {
        /// <summary>
        /// Stores reference to string resources
        /// </summary>
        private static ResourceManager mStrings = new ResourceManager("XslTransformer.Core.Strings.Resources", Assembly.GetExecutingAssembly());

        /// <summary>
        /// Holds a reference to the xslt stylesheet the parameter belongs to
        /// </summary>
        public XsltStylesheet Parent { get; set; }

        /// <summary>
        /// The name of the parameter
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The value of the parameter
        /// </summary>
        public string Value { get; set; }

        // IDataErrorInfo implementation

        public string Error => throw new NotImplementedException();

        /// <summary>
        /// get is invoked everytime the user changes a field in parameter DataGrid
        /// </summary>
        /// <param name="columnName">contains the property name modified by user</param>
        /// <returns>error string or empty string if there is no error</returns>
        public string this[string columnName]
        {
            get
            {
                // When there are no errors an empty string is returned
                string error = string.Empty;

                // only validate parameter Name
                if (columnName == "Name")
                {
                    // parameter name must not be null or empty string
                    if (string.IsNullOrWhiteSpace(Name))
                    {
                        error = mStrings.GetString("ParameterNameMissingError");
                    }
                    else
                    {
                        // parameter name must be unique
                        foreach (var item in Parent.Parameters)
                        {
                            if (!ReferenceEquals(this, item) && item.Name == Name)
                            {
                                error = mStrings.GetString("ParameterNameDoubleError");
                                break;
                            }
                        }
                    }
                }

                return error;
            }
        }
    }
}

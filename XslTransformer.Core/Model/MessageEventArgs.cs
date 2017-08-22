using System;

namespace XslTransformer.Core
{
    public class MessageEventArgs : EventArgs
    {
        /// <summary>
        /// The type of message to display
        /// </summary>
        public MessageType MessageType { get; set; }

        /// <summary>
        /// The path to the processed file (if any)
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// The exception message to be included in message display
        /// </summary>
        public string Message { get; set; }
    }
}

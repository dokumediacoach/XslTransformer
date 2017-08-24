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
        /// The string representations of the objects in the array will
        /// replace the format items in the message (using String.Format method)
        /// </summary>
        public object[] MessageParams { get; set; }
    }
}

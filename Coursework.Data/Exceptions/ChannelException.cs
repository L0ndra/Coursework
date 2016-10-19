using System;
using System.Runtime.Serialization;

namespace Coursework.Data.Exceptions
{
    [Serializable]
    public class ChannelException : Exception
    {
        public ChannelException()
        {
        }

        public ChannelException(string message) : base(message)
        {
        }

        public ChannelException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ChannelException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
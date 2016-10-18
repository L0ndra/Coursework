using System;
using System.Runtime.Serialization;

namespace Coursework.Data.Exceptions
{
    [Serializable]
    public class NodeException : Exception
    {
        public NodeException()
        {
        }

        public NodeException(string message) : base(message)
        {
        }

        public NodeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NodeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
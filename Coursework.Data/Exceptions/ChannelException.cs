using System;

namespace Coursework.Data.Exceptions
{
    public class ChannelException : Exception
    {
        public ChannelException(string message) : base(message)
        {
        }
    }
}
using System;
using Coursework.Data.Exceptions;

namespace Coursework.Data.Services
{
    public class ExceptionCatcher : IExceptionDecorator
    {
        public void Decorate(Action method, Action<Exception> exceptionHandler)
        {
            try
            {
                method();
            }
            catch (Exception ex) when (ex is ChannelException || ex is NodeException ||
                ex is ArgumentNullException || ex is FormatException || ex is OverflowException
                || ex is MessageException || ex is ArgumentException)
            {
                exceptionHandler(ex);
            }
        }
    }
}

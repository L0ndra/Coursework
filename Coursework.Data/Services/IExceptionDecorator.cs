using System;

namespace Coursework.Data.Services
{
    public interface IExceptionDecorator
    {
        void Decorate(Action method, Action<Exception> exceptionHandler);
    }
}

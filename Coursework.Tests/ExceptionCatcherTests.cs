using System;
using Coursework.Data.Services;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    public class ExceptionCatcherTests
    {
        private IExceptionDecorator _exceptionCatcher;

        [SetUp]
        public void Setup()
        {
            _exceptionCatcher = new ExceptionCatcher();
        }

        [Test]
        public void DecorateShouldCallMethod()
        {
            // Arrange
            var i = 0;
            Action action = () => ++i;

            // Act
            _exceptionCatcher.Decorate(action, null);

            // Assert
            Assert.That(i, Is.EqualTo(1));
        }

        [Test]
        public void DecorateShouldThrowNothing()
        {
            // Arrange
            Action action = () =>
            {
                throw new ArgumentException();
            };

            Action<Exception> exceptionHandler = ex => { };

            // Act
            TestDelegate testDelegate = () => _exceptionCatcher.Decorate(action, exceptionHandler);

            // Assert
            Assert.That(testDelegate, Throws.Nothing);
        }

        [Test]
        public void DecorateShouldThrowException()
        {
            // Arrange
            Action action = () =>
            {
                throw new NotImplementedException();
            };

            Action<Exception> exceptionHandler = ex => { };

            // Act
            TestDelegate testDelegate = () => _exceptionCatcher.Decorate(action, exceptionHandler);

            // Assert
            Assert.That(testDelegate, Throws.TypeOf(typeof(NotImplementedException)));
        }
    }
}

using System.Linq;
using Coursework.Data.Builder;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    public class NodeGeneratorTests
    {
        private INodeGenerator _nodeGenerator;

        [SetUp]
        public void Setup()
        {
            _nodeGenerator = new NodeGenerator();    
        }

        [Test]
        [TestCase(5)]
        [TestCase(0)]
        public void GenerateNodesShouldReturnNodeArrayOfSpecifiedCount(int count)
        {
            // Arrange
            // Act
            var result = _nodeGenerator.GenerateNodes(count);

            // Assert
            Assert.That(result.Length, Is.EqualTo(count));
        }

        [Test]
        public void ResetAccumulatorShouldDoItAndReturnNextNodeWithStartId()
        {
            // Arrange
            _nodeGenerator.GenerateNodes(10);

            // Act
            _nodeGenerator.ResetAccumulator();
            var result = _nodeGenerator.GenerateNodes(1).First();

            // Assert
            Assert.That(result.Id, Is.Zero);
        }
    }
}

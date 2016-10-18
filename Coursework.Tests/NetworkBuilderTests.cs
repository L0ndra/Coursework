using Coursework.Data.Builder;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    public class NetworkBuilderTests
    {
        private INetworkBuilder _networkBuilder;

        [SetUp]
        public void Setup()
        {
            _networkBuilder = new NetworkBuilder();
        }

        [Test]
        public void BuildShouldCreateNetworkWithConcreteNumberOfNodesWithLinks()
        {
            Assert.Fail("Not implemented method");
        }
    }
}

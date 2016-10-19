using System;
using System.Linq;
using Coursework.Data.Builder;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    public class NetworkBuilderTests
    {
        private INetworkBuilder _networkBuilder;
        private const int TimesToCreateNetwork = 1000;
        private readonly int[] _availablePrices = { 2, 4, 7, 8, 11, 15, 17, 20, 24, 25, 28 };
        private Random _random;

        [SetUp]
        public void Setup()
        {
            _random = new Random((int)(DateTime.Now.Ticks & 0xFFFF));
        }

        [Test]
        [TestCase((uint)5, 2.0, 0.9)]
        [TestCase((uint)20, 2.0, 0.9)]
        [TestCase((uint)20, 19.0, 19.0)]
        [TestCase((uint)10, 1.0, 0.9)]
        public void BuildShouldCreateCorrectNetworkSpecifiedNumberOfTimesWithSpecifiedParameters(uint nodeCount, double networkPower, double eps)
        {
            for (var i = 0; i < TimesToCreateNetwork; i++)
            {
                // Arrange
                _networkBuilder = new NetworkBuilder(nodeCount, networkPower, _random);

                // Act
                var network = _networkBuilder.Build();
                var currentPower = network.Channels.Length * 2 / (double)nodeCount;
                var isAllPricesIsAvailable = network.Channels
                    .Select(c => c.Price)
                    .All(p => _availablePrices.Contains(p));

                // Assert
                Assert.That(network.Nodes.Length, Is.EqualTo(nodeCount));
                Assert.That(network.Channels.Length, Is.GreaterThan(0));
                Assert.That(isAllPricesIsAvailable, Is.True);
                Assert.That(Math.Abs(currentPower - networkPower), Is.LessThanOrEqualTo(eps));

                LogResult(i, networkPower, currentPower);
            }
        }

        private static void LogResult(int testNumber, double networkPower, double currentPower)
        {
            Console.WriteLine($"Test {testNumber}: ");
            Console.WriteLine($"\tcurrent power = {currentPower};");
            Console.WriteLine($"\tspecified power = {networkPower}");
            Console.WriteLine($"\tdifference ABS = {Math.Abs(networkPower - currentPower)}");
        }
    }
}

using System;
using System.Linq;
using Coursework.Data.Entities;

namespace Coursework.Data.Builder
{
    public class NetworkBuilder : INetworkBuilder
    {
        private INetwork _network;
        private readonly uint _nodeCount;
        private readonly double _networkPower;

        private readonly int[] _availablePrices = { 2, 4, 7, 8, 11, 15, 17, 20, 24, 25, 28 };
        private readonly Random _random = new Random((int)(DateTime.Now.Ticks & 0xFFFF));


        public NetworkBuilder(uint nodeCount, double networkPower)
        {
            if (networkPower <= 0.0)
            {
                throw new ArgumentException("networkPower");
            }

            _nodeCount = nodeCount;
            _networkPower = networkPower;
        }

        public INetwork Build()
        {
            if (_network == null)
            {
                InitializeNetwork();
            }

            return _network;
        }

        private void InitializeNetwork()
        {
            _network = new Network();

            CreateNodes();

            CreateChannels();
        }

        private void CreateChannels()
        {
            var roundedPower = (int)Math.Ceiling(_networkPower);

            foreach (var node in _network.Nodes)
            {
                var currentChannelsNumber = _network.Channels
                    .Count(c => c.SecondNodeId == node.Id);

                var numberOfChannels = _random.Next(roundedPower) + 1 - currentChannelsNumber;

                var maxChannelsCountInNode = Enumerable
                    .Range(0, _network.Nodes.Length)
                    .Count(id => !_network.IsChannelExists(node.Id, (uint)id)) - 1;

                if (numberOfChannels > maxChannelsCountInNode)
                {
                    numberOfChannels = maxChannelsCountInNode;
                }

                for (var i = 0; i < numberOfChannels; i++)
                {
                    GenerateChannel(node.Id);
                }
            }
        }

        private void GenerateChannel(uint currentNodeId)
        {
            uint? destinationNodeId = null;

            while (destinationNodeId == null)
            {
                destinationNodeId = (uint)_random.Next(_network.Nodes.Length);

                if (destinationNodeId == currentNodeId || _network.IsChannelExists(currentNodeId, destinationNodeId.Value))
                {
                    destinationNodeId = null;
                    continue;
                }

                var channel = new Channel
                {
                    FirstNodeId = currentNodeId,
                    SecondNodeId = destinationNodeId.Value,
                    ChannelType = ChannelType.Ground,
                    ConnectionType = ConnectionType.Duplex,
                    ErrorChance = _random.NextDouble(),
                    Price = _availablePrices[_random.Next(_availablePrices.Length)]
                };

                _network.AddChannel(channel);
            }
        }

        private void CreateNodes()
        {
            for (uint i = 0; i < _nodeCount; i++)
            {
                var node = new Node
                {
                    Id = i
                };

                _network.AddNode(node);
            }
        }
    }
}

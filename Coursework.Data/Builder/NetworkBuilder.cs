using System;
using System.Linq;

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
            var maxChannelsCount = (int)_networkPower * 2;

            foreach (var node in _network.Nodes)
            {
                var numberOfChannels = _random.Next(maxChannelsCount) + 1;

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
            uint? toNode = null;

            while (toNode == null)
            {
                toNode = (uint)_random.Next(_network.Nodes.Length);

                if (toNode == currentNodeId || _network.IsChannelExists(currentNodeId, toNode.Value))
                {
                    toNode = null;
                    continue;
                }

                var channel = new Channel
                {
                    FirstNodeId = currentNodeId,
                    SecondNodeId = toNode.Value,
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

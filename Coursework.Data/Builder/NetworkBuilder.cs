using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Coursework.Data.Constants;
using Coursework.Data.Entities;

namespace Coursework.Data.Builder
{
    public class NetworkBuilder : INetworkBuilder
    {
        private INetwork _network;
        private readonly uint _nodeCount;
        private readonly double _networkPower;

        private readonly SortedSet<int> _usedPrices = new SortedSet<int>();

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

            _usedPrices.Clear();

            foreach (var node in _network.Nodes)
            {
                var currentChannelsNumber = _network.Channels
                    .Count(c => c.SecondNodeId == node.Id);

                var numberOfChannels = AllConstants.RandomGenerator.Next(roundedPower) + 1 - currentChannelsNumber;

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
                destinationNodeId = (uint)AllConstants.RandomGenerator.Next(_network.Nodes.Length);

                if (destinationNodeId == currentNodeId || _network.IsChannelExists(currentNodeId, destinationNodeId.Value))
                {
                    destinationNodeId = null;
                    continue;
                }

                var price = GetRandomPrice();

                var channel = new Channel
                {
                    FirstNodeId = currentNodeId,
                    SecondNodeId = destinationNodeId.Value,
                    ChannelType = ChannelType.Ground,
                    ConnectionType = ConnectionType.Duplex,
                    ErrorChance = AllConstants.RandomGenerator.NextDouble(),
                    Price = price
                };

                _network.AddChannel(channel);
            }
        }

        private void CreateNodes()
        {
            foreach (var node in NodeIdGenerator.GenerateNodes(0, (int)_nodeCount))
            {
                _network.AddNode(node);
            }
        }

        private int GetRandomPrice()
        {
            if (!AllConstants.AllPrices.Except(_usedPrices).Any())
            {
                _usedPrices.Clear();
            }

            var price = AllConstants.AllPrices
                .Except(_usedPrices)
                .OrderBy(n => AllConstants.RandomGenerator.Next())
                .FirstOrDefault();

            _usedPrices.Add(price);

            return price;
        }
    }
}

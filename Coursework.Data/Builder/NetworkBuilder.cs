using System;
using System.Linq;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;

namespace Coursework.Data.Builder
{
    public class NetworkBuilder : INetworkBuilder
    {
        private INetworkHandler _network;
        private readonly INodeGenerator _nodeGenerator;
        private readonly uint _nodeCount;
        private readonly double _networkPower;

        public NetworkBuilder(INodeGenerator nodeGenerator, uint nodeCount, double networkPower)
        {
            if (networkPower <= 0.0)
            {
                throw new ArgumentException("networkPower");
            }

            _nodeGenerator = nodeGenerator;
            _nodeCount = nodeCount;
            _networkPower = networkPower;
        }

        public INetworkHandler Build()
        {
            InitializeNetwork();

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

                var numberOfChannels = AllConstants.RandomGenerator.Next(roundedPower) + 1 - currentChannelsNumber;

                var maxChannelsCountInNode = Enumerable
                    .Range(0, _network.Nodes.Length)
                    .Count(id => _network.GetChannel(node.Id, (uint)id) == null) - 1;

                if (numberOfChannels > maxChannelsCountInNode)
                {
                    numberOfChannels = maxChannelsCountInNode;
                }

                for (var i = 0; i < numberOfChannels; i++)
                {
                    var channel = GenerateChannel(node.Id);

                    _network.AddChannel(channel);
                }
            }
        }

        private Channel GenerateChannel(uint currentNodeId)
        {
            while (true)
            {
                var destinationNodeId = _network.Nodes
                    .Select(n => n.Id)
                    .ElementAt(AllConstants.RandomGenerator.Next(_network.Nodes.Length));

                if (destinationNodeId == currentNodeId || _network.GetChannel(currentNodeId, destinationNodeId) != null)
                {
                    continue;
                }

                var price = PriceGenerator.GetRandomPrice();

                var channel = new Channel
                {
                    Id = Guid.NewGuid(),
                    FirstNodeId = currentNodeId,
                    SecondNodeId = destinationNodeId,
                    ChannelType = ChannelType.Ground,
                    ConnectionType = ConnectionType.Duplex,
                    ErrorChance = Math.Round(AllConstants.RandomGenerator.NextDouble() * 100.0) / 100.0,
                    Price = price
                };

                return channel;
            }
        }

        private void CreateNodes()
        {
            foreach (var node in _nodeGenerator.GenerateNodes((int)_nodeCount))
            {
                _network.AddNode(node);
            }

            var centralMachine = _network.Nodes.First();
            centralMachine.NodeType = NodeType.CentralMachine;
        }
    }
}

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
        private readonly int _nodeCount;
        private readonly double _networkPower;

        public NetworkBuilder(INodeGenerator nodeGenerator, int nodeCount, double networkPower)
        {
            if (networkPower <= 0.0)
            {
                throw new ArgumentException("networkPower");
            }
            if (nodeCount <= 0)
            {
                throw new ArgumentException("nodeCount");
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
            foreach (var node in _network.Nodes)
            {
                var numberOfChannels = GetNumberOfChannels(node);

                for (var i = 0; i < numberOfChannels; i++)
                {
                    var channel = GenerateChannel(node.Id);

                    _network.AddChannel(channel);
                }
            }
        }

        private int GetNumberOfChannels(Node node)
        {
            var roundedPower = (int)Math.Ceiling(_networkPower);

            var currentChannelsNumber = _network.Channels.Length;

            var maxChannelsCountInNode = MaxChannelsCountInNode(node);

            var nodesWithChannelsCount = _network.Nodes
                .Select(n => _network.GetChannels(n.Id))
                .Count(c => c.Length != 0);

            var currentNodePower = 2 * currentChannelsNumber / (double)nodesWithChannelsCount;

            int numberOfChannels;

            if (_network.Channels.Length == 0
                || Math.Abs(_networkPower - currentNodePower) < AllConstants.Eps)
            {
                numberOfChannels = AllConstants.RandomGenerator.Next(roundedPower) + 1;
            }
            else
            {
                numberOfChannels = nodesWithChannelsCount * roundedPower - 2 * currentChannelsNumber;

                if (numberOfChannels < 0)
                {
                    numberOfChannels = 0;
                }
            }

            if (numberOfChannels > maxChannelsCountInNode)
            {
                numberOfChannels = maxChannelsCountInNode;
            }

            return numberOfChannels;
        }

        private int MaxChannelsCountInNode(Node node)
        {
            var maxChannelsCountInNode = _network.Nodes
                .Select(checkNode => _network.GetChannel(checkNode.Id, node.Id))
                .Count(channel => channel == null);

            return maxChannelsCountInNode - 1;
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

                var price = ChannelParamsGenerator.GetRandomPrice();
                var capacity = ChannelParamsGenerator.GetCapactity(price);

                var channel = new Channel
                {
                    Id = Guid.NewGuid(),
                    FirstNodeId = currentNodeId,
                    SecondNodeId = destinationNodeId,
                    ChannelType = ChannelType.Ground,
                    ConnectionType = ConnectionType.Duplex,
                    ErrorChance = Math.Floor(AllConstants.RandomGenerator.NextDouble() * 100.0) / 100.0,
                    Price = price,
                    IsBusy = false,
                    Capacity = capacity
                };

                return channel;
            }
        }

        private void CreateNodes()
        {
            foreach (var node in _nodeGenerator.GenerateNodes(_nodeCount))
            {
                _network.AddNode(node);
            }

            var centralMachine = _network.Nodes.First();
            centralMachine.NodeType = NodeType.CentralMachine;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.MessageServices;
using Coursework.Data.NetworkData;

namespace Coursework.Data.Builder
{
    public class WideAreaNetworkBuilder : INetworkBuilder
    {
        private readonly INetworkBuilder _simpleNetworkBuilder;
        private readonly int _numberOfMetropolitanNetworks;
        private INetworkHandler _network;

        public WideAreaNetworkBuilder(INetworkBuilder simpleNetworkBuilder, int numberOfMetropolitanNetworks)
        {
            _simpleNetworkBuilder = simpleNetworkBuilder;
            _numberOfMetropolitanNetworks = numberOfMetropolitanNetworks;
        }

        public INetworkHandler Build()
        {
            var metropolianNetworks = CreateMetropolianNetworks();

            ConvertMetropolianNetworksToWideAreaNetwork(metropolianNetworks);

            var centralMachine = GenerateCentralMachineNode();

            _network.AddNode(centralMachine);

            var nodesIdConnectedToCentralMachine = CreateSetOfConnectedToCentralMachineNodesId(metropolianNetworks);

            ConnectCentralMachineToOtherNodes(nodesIdConnectedToCentralMachine, centralMachine.Id);

            return _network;
        }

        private Node GenerateCentralMachineNode()
        {
            var centralMachine = new Node
            {
                Id = _network.Nodes.Max(n => n.Id) + 1,
                LinkedNodesId = new SortedSet<uint>(),
                MessageQueueHandlers = new List<MessageQueueHandler>(),
                NodeType = NodeType.CentralMachine,
                IsActive = false
            };

            return centralMachine;
        }

        private IEnumerable<uint> CreateSetOfConnectedToCentralMachineNodesId(IList<INetworkHandler> metropolianNetworks)
        {
            var result = new SortedSet<uint>();

            foreach (var metropolianNetwork in metropolianNetworks)
            {
                var nodesCount = metropolianNetwork.Nodes.Length;
                var randomId = metropolianNetwork.Nodes
                    .ElementAt(AllConstants.RandomGenerator.Next(nodesCount)).Id;

                result.Add(randomId);
            }

            return result;
        }

        private void ConnectCentralMachineToOtherNodes(IEnumerable<uint> nodesIdConnectedToCentralMachine, uint centralMachineId)
        {
            foreach (var nodeId in nodesIdConnectedToCentralMachine)
            {
                var channel = new Channel
                {
                    Id = Guid.NewGuid(),
                    FirstNodeId = centralMachineId,
                    SecondNodeId = nodeId,
                    ConnectionType = ConnectionType.Duplex,
                    ChannelType = ChannelType.Satellite,
                    ErrorChance = AllConstants.RandomGenerator.NextDouble(),
                    Price = PriceGenerator.GetRandomPrice()
                };

                var metropolitanNode = _network.GetNodeById(nodeId);
                metropolitanNode.NodeType = NodeType.MainMetropolitanMachine;

                _network.AddChannel(channel);
            }
        }

        private IList<INetworkHandler> CreateMetropolianNetworks()
        {
            var metropolitanNetworks = new List<INetworkHandler>();

            for (var i = 0; i < _numberOfMetropolitanNetworks; i++)
            {
                var network = _simpleNetworkBuilder.Build();

                metropolitanNetworks.Add(network);
            }

            return metropolitanNetworks;
        }

        private void ConvertMetropolianNetworksToWideAreaNetwork(IEnumerable<INetworkHandler> metropolitanNetworks)
        {
            _network = new Network();

            foreach (var metropolitanNetwork in metropolitanNetworks)
            {
                foreach (var node in metropolitanNetwork.Nodes)
                {
                    node.NodeType = NodeType.SimpleNode;
                    _network.AddNode(node);
                }

                foreach (var channel in metropolitanNetwork.Channels)
                {
                    _network.AddChannel(channel);
                }
            }
        }
    }
}

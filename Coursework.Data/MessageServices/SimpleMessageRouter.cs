using System;
using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;

namespace Coursework.Data.MessageServices
{
    public class SimpleMessageRouter : IMessageRouter
    {
        private class NetworkMatrix
        {
            public readonly IDictionary<uint, double> NodeIdWithCurrentPrice = new Dictionary<uint, double>();
            public readonly SortedSet<uint> VisitedNodes = new SortedSet<uint>();

            public static NetworkMatrix Initialize(INetwork network)
            {
                var networkMatrix = new NetworkMatrix();

                foreach (var node in network.Nodes)
                {
                    networkMatrix.NodeIdWithCurrentPrice[node.Id] = double.PositiveInfinity;
                }

                return networkMatrix;
            }
        }

        protected readonly INetworkHandler Network;

        public SimpleMessageRouter(INetworkHandler network)
        {
            Network = network;
        }

        public Channel[] GetRoute(uint senderId, uint receiverId)
        {
            if (senderId == receiverId)
            {
                return new Channel[0];
            }

            var networkMatrix = NetworkMatrix.Initialize(Network);
            networkMatrix.NodeIdWithCurrentPrice[senderId] = 0.0;

            CountPriceMatrix(senderId, receiverId, networkMatrix);

            if (!networkMatrix.VisitedNodes.Contains(receiverId))
            {
                return null;
            }

            var route = BuildRoute(networkMatrix, senderId, receiverId);
            return route.ToArray();
        }

        protected virtual double CountPrice(uint startId, uint destinationId)
        {
            var startNode = Network.GetNodeById(startId);
            var destinationNode = Network.GetNodeById(destinationId);

            if (!startNode.IsActive || !destinationNode.IsActive)
            {
                return double.PositiveInfinity;
            }

            var channel = Network.GetChannel(startId, destinationId);

            var startMessageQueue = startNode.MessageQueueHandlers
                .First(m => m.ChannelId == channel.Id);

            return channel.Price * (channel.ErrorChance + 1)
                   * (channel.ErrorChance + 1)
                   * (startMessageQueue.MessagesCount + 1)
                   * (startMessageQueue.MessagesCount + 1);
        }

        private List<Channel> BuildRoute(NetworkMatrix networkMatrix, uint senderId, uint receiverId)
        {
            var currentNodeId = receiverId;

            var route = new List<Channel>();

            while (currentNodeId != senderId)
            {
                var currentNode = Network.GetNodeById(currentNodeId);

                foreach (var linkedNodeId in currentNode.LinkedNodesId)
                {
                    var difference = Math.Abs(networkMatrix.NodeIdWithCurrentPrice[currentNodeId]
                                              - networkMatrix.NodeIdWithCurrentPrice[linkedNodeId]
                                              - CountPrice(currentNodeId, linkedNodeId));

                    if (difference < AllConstants.Eps)
                    {
                        var channel = Network.GetChannel(currentNodeId, linkedNodeId);
                        route.Add(channel);

                        currentNodeId = linkedNodeId;
                        break;
                    }
                }
            }

            route.Reverse();
            return route;
        }

        private void CountPriceMatrix(uint currentId, uint receiverId, NetworkMatrix matrix)
        {
            if (Network.Nodes.All(n => matrix.VisitedNodes.Contains(n.Id))
                || matrix.VisitedNodes.Contains(currentId)
                || currentId == receiverId)
            {
                matrix.VisitedNodes.Add(currentId);
                return;
            }

            matrix.VisitedNodes.Add(currentId);
            var currentNode = Network.GetNodeById(currentId);

            foreach (var linkedNodeId in currentNode.LinkedNodesId)
            {
                var currentPrice = matrix.NodeIdWithCurrentPrice[currentId] + CountPrice(currentId, linkedNodeId);

                if (matrix.NodeIdWithCurrentPrice[linkedNodeId] > currentPrice)
                {
                    matrix.NodeIdWithCurrentPrice[linkedNodeId] = currentPrice;
                }
            }

            var nextNodeId = matrix.NodeIdWithCurrentPrice
                .Where(kv => !matrix.VisitedNodes.Contains(kv.Key))
                .Aggregate((l, r) => l.Value < r.Value ? l : r)
                .Key;

            if (!double.IsInfinity(matrix.NodeIdWithCurrentPrice[nextNodeId]))
            {
                CountPriceMatrix(nextNodeId, receiverId, matrix);
            }
        }
    }
}

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

            var sender = Network.GetNodeById(senderId);

            var networkMatrix = sender.NetworkMatrix;

            if (double.IsInfinity(networkMatrix.NodeIdWithCurrentPrice[receiverId]))
            {
                return null;
            }

            var route = BuildRoute(networkMatrix, senderId, receiverId);

            return route.ToArray();
        }

        public NetworkMatrix CountPriceMatrix(uint currentId, NetworkMatrix matrix = null,
            SortedSet<uint> visitedNodes = null)
        {
            if (visitedNodes == null || matrix == null)
            {
                matrix = NetworkMatrix.Initialize(Network);
                matrix.NodeIdWithCurrentPrice[currentId] = 0.0;
                visitedNodes = new SortedSet<uint>();
            }

            if (visitedNodes.Contains(currentId))
            {
                visitedNodes.Add(currentId);
                return matrix;
            }

            visitedNodes.Add(currentId);
            var currentNode = Network.GetNodeById(currentId);

            foreach (var linkedNodeId in currentNode.LinkedNodesId)
            {
                matrix.PriceMatrix[currentId][linkedNodeId] = CountPrice(currentId, linkedNodeId);

                var currentPrice = matrix.NodeIdWithCurrentPrice[currentId]
                    + matrix.PriceMatrix[currentId][linkedNodeId];

                if (matrix.NodeIdWithCurrentPrice[linkedNodeId] > currentPrice)
                {
                    matrix.NodeIdWithCurrentPrice[linkedNodeId] = currentPrice;
                }
            }

            if (!Network.Nodes.All(n => visitedNodes.Contains(n.Id)))
            {
                var nextNodeId = matrix.NodeIdWithCurrentPrice
                .Where(kv => !visitedNodes.Contains(kv.Key))
                .Aggregate((l, r) => l.Value < r.Value ? l : r)
                .Key;

                if (!double.IsInfinity(matrix.NodeIdWithCurrentPrice[nextNodeId]))
                {
                    return CountPriceMatrix(nextNodeId, matrix, visitedNodes);
                }
            }

            return matrix;
        }

        public virtual double CountPrice(uint startId, uint destinationId)
        {
            if (startId == destinationId)
            {
                return 0.0;
            }

            var startNode = Network.GetNodeById(startId);
            var destinationNode = Network.GetNodeById(destinationId);

            var channel = Network.GetChannel(startId, destinationId);

            if (!startNode.IsActive || !destinationNode.IsActive || channel == null)
            {
                return double.PositiveInfinity;
            }

            return channel.Price;
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
                                              - networkMatrix.PriceMatrix[linkedNodeId][currentNodeId]);

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
    }
}

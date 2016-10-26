using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Entities;

namespace Coursework.Data.Services
{
    public class WideAreaNetworkService : IWideAreaNetworkService
    {
        private readonly INetworkHandler _network;

        public WideAreaNetworkService(INetworkHandler network)
        {
            _network = network;
        }

        public Node[][] GetNodesInOneMetropolitanNetwork()
        {
            var groups = new List<Node[]>();
            var currentNode = _network.Nodes.FirstOrDefault();

            while (currentNode != null)
            {
                var group = new List<Node> {currentNode};
                SearchConnectedNodes(group, currentNode);

                groups.Add(group.ToArray());

                currentNode = _network.Nodes
                    .FirstOrDefault(n => !groups.Any(g => g.Any(n1 => n1.Id == n.Id)));
            }

            return groups.ToArray();
        }

        private void SearchConnectedNodes(ICollection<Node> currentNetwork, Node node)
        {
            foreach (var linkedNodeId in node.LinkedNodesId)
            {
                var channel = _network.GetChannel(node.Id, linkedNodeId);

                if (currentNetwork.Any(n => n.Id == linkedNodeId) || channel.ChannelType == ChannelType.Satellite)
                {
                    continue;
                }

                var linkedNode = _network.GetNodeById(linkedNodeId);
                currentNetwork.Add(linkedNode);

                SearchConnectedNodes(currentNetwork, linkedNode);
            }
        }
    }
}

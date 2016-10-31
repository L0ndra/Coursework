using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using Coursework.Data.Entities;
using Coursework.Data.MessageServices;
using Coursework.Data.NetworkData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Coursework.Data.IONetwork
{
    public class NetworkInfoRetriever : INetworkInfoRetriever
    {
        public INetwork Read(string filename)
        {
            using (var file = new StreamReader(filename))
            {
                var jsonNetwork = file.ReadToEnd();

                var network = ParseString(jsonNetwork);

                return network;
            }
        }

        public void Write(string filename, INetwork network)
        {
            var networkIoDto = ConvertToNetworkIoDto(network);

            var jsonNetwork = JsonConvert.SerializeObject(networkIoDto, Formatting.Indented);

            using (var file = new StreamWriter(filename))
            {
                file.Write(jsonNetwork);
            }
        }

        private INetwork ParseString(string jsonNetwork)
        {
            var network = new Network();

            var unparsedNetwork = JObject.Parse(jsonNetwork);

            ParseNodes(unparsedNetwork, network);
            ParseChannels(unparsedNetwork, network);

            return network;
        }

        private void ParseNodes(JObject unparsedNetwork, INetworkHandler network)
        {
            foreach (var nodeInfo in unparsedNetwork["NodeIoDtos"])
            {
                var node = new Node
                {
                    Id = (uint)nodeInfo["Id"],
                    LinkedNodesId = new SortedSet<uint>(),
                    MessageQueueHandlers = new List<MessageQueueHandler>(),
                    NodeType = (NodeType)(int)nodeInfo["NodeType"],
                    IsActive = false
                };

                network.AddNode(node);
            }
        }

        private void ParseChannels(JObject unparsedNetwork, INetworkHandler network)
        {
            foreach (var channelInfo in unparsedNetwork["ChannelIoDtos"])
            {
                var channel = new Channel
                {
                    Id = (Guid)channelInfo["Id"],
                    Price = (int)channelInfo["Price"],
                    ErrorChance = (double)channelInfo["ErrorChance"],
                    FirstNodeId = (uint)channelInfo["FirstNodeId"],
                    SecondNodeId = (uint)channelInfo["SecondNodeId"],
                    ConnectionType = (ConnectionType)(int)channelInfo["ConnectionType"],
                    ChannelType = (ChannelType)(int)channelInfo["ChannelType"],
                    IsFree = true
                };

                network.AddChannel(channel);
            }
        }

        private NetworkIoDto ConvertToNetworkIoDto(INetwork network)
        {
            var channelIoDtos = network.Channels
                .Select(Mapper.Map<Channel, ChannelIoDto>)
                .ToArray();

            var nodeIoDtos = network.Nodes
                .Select(Mapper.Map<Node, NodeIoDto>)
                .ToArray();

            return new NetworkIoDto
            {
                ChannelIoDtos = channelIoDtos,
                NodeIoDtos = nodeIoDtos
            };
        }
    }
}

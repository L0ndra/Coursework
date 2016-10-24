using System;
using System.Collections.Generic;
using System.IO;
using Coursework.Data.Entities;
using Coursework.Data.MessageServices;
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
            var jsonNetwork = JsonConvert.SerializeObject(network, Formatting.Indented);

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
            foreach (var nodeInfo in unparsedNetwork["Nodes"])
            {
                var node = new Node
                {
                    Id = (uint)nodeInfo["Id"],
                    LinkedNodesId = new SortedSet<uint>(),
                    MessageQueue = new MessageQueueHandler()
                };

                network.AddNode(node);
            }
        }

        private void ParseChannels(JObject unparsedNetwork, INetworkHandler network)
        {
            foreach (var channelInfo in unparsedNetwork["Channels"])
            {
                var channel = new Channel
                {
                    Id = (Guid)channelInfo["Id"],
                    Price = (int)channelInfo["Price"],
                    ErrorChance = (double)channelInfo["ErrorChance"],
                    FirstNodeId = (uint)channelInfo["FirstNodeId"],
                    SecondNodeId = (uint)channelInfo["SecondNodeId"],
                    ConnectionType = (ConnectionType)(int)channelInfo["ConnectionType"],
                    ChannelType = (ChannelType)(int)channelInfo["ChannelType"]
                };

                network.AddChannel(channel);
            }
        }
    }
}

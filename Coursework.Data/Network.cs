﻿using System;
using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Entities;
using Coursework.Data.Exceptions;
using Coursework.Data.MessageServices;

namespace Coursework.Data
{
    public class Network : INetworkHandler
    {
        private readonly IList<Node> _nodes;
        private readonly IList<Channel> _channels;
        public Node[] Nodes => _nodes.ToArray();
        public Channel[] Channels => _channels.ToArray();

        public Network()
        {
            _nodes = new List<Node>();
            _channels = new List<Channel>();
        }

        public void AddNode(Node node)
        {
            ThrowExceptionIfNodeCannotBeCreated(node);

            _nodes.Add(node);
        }

        public void RemoveNode(uint nodeId)
        {
            var node = GetNodeById(nodeId);

            if (node != null)
            {
                var channels = GetChannels(nodeId);

                foreach (var channel in channels)
                {
                    _channels.Remove(channel);
                }

                _nodes.Remove(node);
            }

        }

        public Channel[] GetChannels(uint nodeId)
        {
            var node = GetNodeById(nodeId);

            var channels = node
                .LinkedNodesId
                .Select(linkedNodeId => GetChannel(nodeId, linkedNodeId))
                .ToArray();

            return channels;
        }

        public void AddChannel(Channel channel)
        {
            ThrowExceptionIfChannelCannotBeCreated(channel);

            AddLinkToNodes(channel.FirstNodeId, channel.SecondNodeId);
            AddMessageQueueToNodes(channel);
            
            _channels.Add(channel);
        }

        public void UpdateChannel(Channel newChannel)
        {
            var oldChannel = GetChannel(newChannel.FirstNodeId, newChannel.SecondNodeId);

            _channels.Remove(oldChannel);
            AddChannel(newChannel);
        }

        public void RemoveChannel(uint firstNodeId, uint secondNodeId)
        {
            var channel = GetChannel(firstNodeId, secondNodeId);

            _channels.Remove(channel);
        }

        public Channel GetChannel(uint firstNodeId, uint secondNodeId)
        {
            return Channels.FirstOrDefault(c => c.FirstNodeId == firstNodeId && c.SecondNodeId == secondNodeId
                                     || c.FirstNodeId == secondNodeId && c.SecondNodeId == firstNodeId);
        }

        private void AddMessageQueueToNodes(Channel channel)
        {
            var node1 = GetNodeById(channel.FirstNodeId);
            var node2 = GetNodeById(channel.SecondNodeId);

            var firstMessageQueue = new MessageQueueHandler(channel.Id);
            var secondMessageQueue = new MessageQueueHandler(channel.Id);

            node1.MessageQueue.Add(firstMessageQueue);
            node2.MessageQueue.Add(secondMessageQueue);
        }

        private void ThrowExceptionIfChannelCannotBeCreated(Channel channel)
        {
            ThrowExceptionIfNodeNotExists(channel.FirstNodeId);
            ThrowExceptionIfNodeNotExists(channel.SecondNodeId);
            ThrowExceptionIfPriceIsIncorrect(channel.Price);
            ThrowExceptionIfErrorChanceIsIncorrect(channel.ErrorChance);
            ThrowExceptionIfChannelWithSameIdExists(channel.Id);
        }

        private void ThrowExceptionIfChannelWithSameIdExists(Guid channelId)
        {
            if (_channels.Any(c => c.Id == channelId))
            {
                throw new ChannelException("Channel with same id is already exists in network");
            }
        }

        private void ThrowExceptionIfNodeCannotBeCreated(Node node)
        {
            ThrowExceptionIfNodeExists(node.Id);
        }

        private void ThrowExceptionIfErrorChanceIsIncorrect(double errorChance)
        {
            if (errorChance < 0.0 || errorChance > 1.0)
            {
                throw new ChannelException("Error chance is in incorrect range");
            }
        }

        private void ThrowExceptionIfPriceIsIncorrect(int price)
        {
            if (price <= 0)
            {
                throw new ChannelException("Price is less or equal to 0");
            }
        }

        private void ThrowExceptionIfNodeNotExists(uint nodeId)
        {
            if (_nodes.All(n => n.Id != nodeId))
            {
                throw new NodeException("Node isn't exists");
            }
        }

        private void ThrowExceptionIfNodeExists(uint nodeId)
        {
            if (_nodes.Any(n => n.Id == nodeId))
            {
                throw new NodeException("Node is already exists");
            }
        }

        private Node GetNodeById(uint id)
        {
            return _nodes.FirstOrDefault(n => n.Id == id);
        }

        private void AddLinkToNodes(uint firstNodeId, uint secondNodeId)
        {
            GetNodeById(firstNodeId).LinkedNodesId.Add(secondNodeId);
            GetNodeById(secondNodeId).LinkedNodesId.Add(firstNodeId);
        }
    }
}

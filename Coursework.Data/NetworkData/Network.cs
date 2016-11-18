using System;
using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Entities;
using Coursework.Data.Exceptions;
using Coursework.Data.MessageServices;

namespace Coursework.Data.NetworkData
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

            node.MessageQueueHandlers = new List<MessageQueueHandler>();
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
                    RemoveChannel(channel.FirstNodeId, channel.SecondNodeId);
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

        public void AddInQueue(Message message, uint nodeId)
        {
            var firstChannel = message.Route.First();

            var node = GetNodeById(nodeId);

            var messageQueue = node.MessageQueueHandlers
                .First(m => m.ChannelId == firstChannel.Id);

            messageQueue.AppendMessage(message);
        }

        public void RemoveFromQueue(Message message, uint nodeId)
        {
            var firstChannel = message.Route.First();

            var node = GetNodeById(nodeId);

            var messageQueue = node.MessageQueueHandlers
                .First(m => m.ChannelId == firstChannel.Id);

            messageQueue.RemoveMessage(message);
        }

        public void ClearMessages()
        {
            ClearNodes();

            ClearChannels();
        }

        public void Reset()
        {
            foreach (var node in _nodes)
            {
                node.IsTableUpdated = false;
                node.NetworkMatrix = null;
            }

            foreach (var channel in _channels)
            {
                channel.IsBusy = false;
            }
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
            ThrowExceptionIfChannelWithSameIdNotExists(newChannel.Id);

            var oldChannel = _channels.FirstOrDefault(c => c.Id == newChannel.Id);

            if (oldChannel != null)
            {
                ThrowExceptionIfChannelCannotBeUpdated(newChannel);

                oldChannel.ChannelType = newChannel.ChannelType;
                oldChannel.ConnectionType = newChannel.ConnectionType;
                oldChannel.ErrorChance = newChannel.ErrorChance;
                oldChannel.Price = newChannel.Price;
                oldChannel.FirstNodeId = newChannel.FirstNodeId;
                oldChannel.SecondNodeId = newChannel.SecondNodeId;
                oldChannel.Capacity = newChannel.Capacity;
            }
        }

        public void RemoveChannel(uint firstNodeId, uint secondNodeId)
        {
            var firstNode = GetNodeById(firstNodeId);
            var secondNode = GetNodeById(secondNodeId);

            var channel = GetChannel(firstNodeId, secondNodeId);

            if (channel != null)
            {
                RemoveMessageQueue(firstNodeId, channel.Id);
                RemoveMessageQueue(secondNodeId, channel.Id);

                _channels.Remove(channel);

                firstNode.LinkedNodesId.Remove(secondNodeId);
                secondNode.LinkedNodesId.Remove(firstNodeId);
            }
        }

        public Node GetNodeById(uint id)
        {
            return _nodes.FirstOrDefault(n => n.Id == id);
        }

        public Channel GetChannel(uint firstNodeId, uint secondNodeId)
        {
            return Channels.FirstOrDefault(c => c.FirstNodeId == firstNodeId && c.SecondNodeId == secondNodeId
                                                || c.FirstNodeId == secondNodeId && c.SecondNodeId == firstNodeId);
        }

        private void RemoveMessageQueue(uint nodeId, Guid channelId)
        {
            var node = GetNodeById(nodeId);

            var messageQueueHandler = node.MessageQueueHandlers
                .First(m => channelId == m.ChannelId);

            node.MessageQueueHandlers
                .Remove(messageQueueHandler);
        }

        private void ClearChannels()
        {
            foreach (var channel in _channels)
            {
                channel.FirstMessage = null;
                channel.SecondMessage = null;
            }
        }

        private void ClearNodes()
        {
            foreach (var node in _nodes)
            {
                node.ReceivedMessages.Clear();
                node.CanceledMessages.Clear();

                foreach (var messageQueueHandler in node.MessageQueueHandlers)
                {
                    var messages = messageQueueHandler.Messages
                        .ToList();

                    messages.ForEach(m => messageQueueHandler.RemoveMessage(m));
                }
            }
        }

        private void AddMessageQueueToNodes(Channel channel)
        {
            var node1 = GetNodeById(channel.FirstNodeId);
            var node2 = GetNodeById(channel.SecondNodeId);

            var firstMessageQueue = new MessageQueueHandler(channel.Id);
            var secondMessageQueue = new MessageQueueHandler(channel.Id);

            node1.MessageQueueHandlers.Add(firstMessageQueue);
            node2.MessageQueueHandlers.Add(secondMessageQueue);
        }

        private void ThrowExceptionIfChannelCannotBeCreated(Channel channel)
        {
            ThrowExceptionIfChannelCannotBeUpdated(channel);
            ThrowExceptionIfChannelWithSameIdExists(channel.Id);
        }

        private void ThrowExceptionIfChannelCannotBeUpdated(Channel channel)
        {
            ThrowExceptionIfNodeNotExists(channel.FirstNodeId);
            ThrowExceptionIfNodeNotExists(channel.SecondNodeId);
            ThrowExceptionIfPriceIsIncorrect(channel.Price);
            ThrowExceptionIfErrorChanceIsIncorrect(channel.ErrorChance);
            ThrowExceptionIfCapacityIsLessOrEqualThanZero(channel.Capacity);
        }

        private void ThrowExceptionIfCapacityIsLessOrEqualThanZero(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ChannelException("Channel capacity is less or equal to zero");
            }
        }

        private void ThrowExceptionIfChannelWithSameIdExists(Guid channelId)
        {
            if (_channels.Any(c => c.Id == channelId))
            {
                throw new ChannelException("Channel with same id is already exist in network");
            }
        }

        private void ThrowExceptionIfChannelWithSameIdNotExists(Guid channelId)
        {
            if (_channels.All(c => c.Id != channelId))
            {
                throw new ChannelException("Channel with same id isn't exist in network");
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
                throw new NodeException("Node isn't exist");
            }
        }

        private void ThrowExceptionIfNodeExists(uint nodeId)
        {
            if (_nodes.Any(n => n.Id == nodeId))
            {
                throw new NodeException("Node is already exist");
            }
        }

        private void AddLinkToNodes(uint firstNodeId, uint secondNodeId)
        {
            GetNodeById(firstNodeId).LinkedNodesId.Add(secondNodeId);
            GetNodeById(secondNodeId).LinkedNodesId.Add(firstNodeId);
        }
    }
}

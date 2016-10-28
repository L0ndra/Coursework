using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;

namespace Coursework.Data.MessageServices
{
    public class MessageExchanger : IMessageExchanger
    {
        private readonly INetworkHandler _network;
        private List<Message> _handledMessages;

        public MessageExchanger(INetworkHandler network)
        {
            _network = network;
        }

        public void Initialize()
        {
            var centralMachine = _network.Nodes
                .FirstOrDefault(n => n.NodeType == NodeType.CentralMachine);

            if (centralMachine != null)
            {
                centralMachine.IsActive = true;

                foreach (var receiverId in centralMachine.LinkedNodesId)
                {
                    CreateInitializeMessage(centralMachine, receiverId);
                }
            }
        }

        public void HandleMessagesOnce()
        {
            _handledMessages = new List<Message>();

            foreach (var node in _network.Nodes)
            {
                HandleMessagesInNodeQueues(node);
            }

            foreach (var channel in _network.Channels)
            {
                HandleMessageInChannel(channel);
            }
        }

        private void HandleMessageInChannel(Channel channel)
        {
            var firstMessage = channel.FirstMessage;
            var secondMessage = channel.SecondMessage;

            if (firstMessage != null
                && !_handledMessages.Contains(firstMessage))
            {
                ReplaceMessageToQueue(channel, firstMessage);
                channel.FirstMessage = null;
            }

            else if (secondMessage != null
                    && !_handledMessages.Contains(secondMessage))
            {
                ReplaceMessageToQueue(channel, secondMessage);
                channel.SecondMessage = null;
            }
        }

        private void ReplaceMessageToQueue(Channel channel, Message message)
        {
            Node node;

            var isSuccess = AllConstants.RandomGenerator.NextDouble() > channel.ErrorChance;

            if (message.LastTransferNodeId == channel.FirstNodeId && !isSuccess
                || message.LastTransferNodeId != channel.FirstNodeId && isSuccess)
            {
                node = _network.GetNodeById(channel.FirstNodeId);
            }
            else
            {
                node = _network.GetNodeById(channel.SecondNodeId);
            }

            var messageQueueHandler = node.MessageQueueHandlers
                .First(m => m.ChannelId == channel.Id);

            messageQueueHandler.AddMessage(message);
        }

        private void HandleMessagesInNodeQueues(Node node)
        {
            foreach (var messageQueueHandler in node.MessageQueueHandlers)
            {
                var currentMessage = messageQueueHandler.Messages
                    .FirstOrDefault();

                var currentChannel = _network.Channels
                    .First(c => c.Id == messageQueueHandler.ChannelId);

                if (currentMessage != null && !_handledMessages.Contains(currentMessage))
                {
                    if (currentMessage.LastTransferNodeId == node.Id)
                    {
                        var result = TryMoveMessageToChannel(currentChannel, currentMessage);

                        if (result)
                        {
                            messageQueueHandler.RemoveMessage(currentMessage);
                            _handledMessages.Add(currentMessage);
                        }
                    }
                    else if (currentMessage.ReceiverId == node.Id)
                    {
                        HandleReceivedMessage(currentMessage);
                        messageQueueHandler.RemoveMessage(currentMessage);
                    }
                    else
                    {
                        ReTransferMessage(currentMessage, node);

                        messageQueueHandler.RemoveMessage(currentMessage);
                    }
                }
            }
        }

        private void ReTransferMessage(Message message, Node node)
        {
            message.LastTransferNodeId = node.Id;

            message.Route = message.Route
                .Skip(1)
                .DefaultIfEmpty()
                .ToArray();

            if (message.Route.Length != 0)
            {
                var destinationMessageQueue = node.MessageQueueHandlers
                    .First(m => m.ChannelId == message.Route[0].Id);

                destinationMessageQueue.AddMessage(message);

                _handledMessages.Add(message);
            }
        }

        private void HandleReceivedMessage(Message message)
        {
            if (message.MessageType == MessageType.InitializeMessage)
            {
                var currentNode = _network.GetNodeById(message.ReceiverId);

                currentNode.IsActive = true;

                InitializeLinkedNodes(currentNode);
            }
        }

        private void InitializeLinkedNodes(Node node)
        {
            foreach (var linkedNodeId in node.LinkedNodesId)
            {
                var linkedNode = _network.GetNodeById(linkedNodeId);

                if (!linkedNode.IsActive)
                {
                    CreateInitializeMessage(node, linkedNodeId);
                }
            }
        }

        private bool TryMoveMessageToChannel(Channel channel, Message message)
        {
            if (channel.ConnectionType == ConnectionType.HalfDuplex
                && channel.FirstMessage == null)
            {
                channel.FirstMessage = message;
                return true;
            }
            if (channel.ConnectionType == ConnectionType.Duplex
                && (channel.FirstMessage == null ||
                channel.SecondMessage == null))
            {
                if (channel.FirstMessage == null)
                {
                    channel.FirstMessage = message;
                }
                else
                {
                    channel.SecondMessage = message;
                }
                return true;
            }

            return false;
        }

        private void CreateInitializeMessage(Node sender, uint receiverId)
        {
            var channel = _network.GetChannel(receiverId, sender.Id);

            var messageQueue = sender.MessageQueueHandlers
                .First(m => m.ChannelId == channel.Id);

            var message = new Message
            {
                ReceiverId = receiverId,
                MessageType = MessageType.InitializeMessage,
                SenderId = sender.Id,
                LastTransferNodeId = sender.Id,
                Size = AllConstants.InitializeMessageSize,
                Data = null,
                Route = new[]
                {
                    channel
                }
            };

            messageQueue.AddMessage(message);
        }
    }
}

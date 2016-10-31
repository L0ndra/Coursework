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
        private readonly IMessageReceiver _messageReceiver;
        private readonly IMessageSender _messageSender;
        private List<Message> _handledMessagesInNode;

        public MessageExchanger(INetworkHandler network, IMessageSender messageSender,
            IMessageReceiver messageReceiver)
        {
            _network = network;
            _messageReceiver = messageReceiver;
            _messageSender = messageSender;
            _handledMessagesInNode = new List<Message>();
        }

        public void Initialize()
        {
            var centralMachine = _network.Nodes
                .FirstOrDefault(n => n.NodeType == NodeType.CentralMachine);

            if (centralMachine != null)
            {
                centralMachine.IsActive = true;

                foreach (var linkedNodeId in centralMachine.LinkedNodesId)
                {
                    var initializeMessage = CreateInitializeMessage(centralMachine.Id, linkedNodeId);

                    _messageSender.StartSendProcess(initializeMessage);
                }
            }
        }

        public void HandleMessagesOnce()
        {
            _handledMessagesInNode = new List<Message>();

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
                && !_handledMessagesInNode.Contains(firstMessage))
            {
                ReplaceMessageToQueue(channel, firstMessage);
                channel.FirstMessage = null;
            }

            else if (secondMessage != null
                    && !_handledMessagesInNode.Contains(secondMessage))
            {
                ReplaceMessageToQueue(channel, secondMessage);
                channel.SecondMessage = null;
            }
        }

        private void HandleMessagesInNodeQueues(Node node)
        {
            ClenoutMessagesInNode(node);

            foreach (var messageQueueHandler in node.MessageQueueHandlers)
            {
                var currentMessage = messageQueueHandler.Messages
                    .FirstOrDefault();

                var currentChannel = _network.Channels
                    .First(c => c.Id == messageQueueHandler.ChannelId);

                if (currentMessage == null || _handledMessagesInNode.Contains(currentMessage))
                {
                    continue;
                }

                if (currentMessage.LastTransferNodeId == node.Id)
                {
                    var isSuccess = TryMoveMessageToChannel(currentChannel, currentMessage);

                    if (isSuccess)
                    {
                        _handledMessagesInNode.Add(currentMessage);
                        messageQueueHandler.RemoveMessage(currentMessage);
                    }
                }
                else
                {
                    _messageReceiver.HandleReceivedMessage(node, currentMessage);
                    messageQueueHandler.RemoveMessage(currentMessage);
                    _handledMessagesInNode.Add(currentMessage);
                }
            }
        }

        private void ClenoutMessagesInNode(Node node)
        {
            foreach (var messageQueueHandler in node.MessageQueueHandlers)
            {
                var messagesToRemove = messageQueueHandler.Messages
                    .Where(message => message.SendAttempts >= AllConstants.MaxAttempts);

                var outdatedMessages = messageQueueHandler.Messages
                    .Where(message => message.MessageType == MessageType.InitializeMessage)
                    .Where(message => _network.GetNodeById(message.ReceiverId).IsActive);

                foreach (var message in messagesToRemove.Union(outdatedMessages))
                {
                    messageQueueHandler.RemoveMessage(message);
                }
            }
        }

        private void ReplaceMessageToQueue(Channel channel, Message message)
        {
            Node node;

            var isSuccess = AllConstants.RandomGenerator.NextDouble() >= channel.ErrorChance;

            if (message.LastTransferNodeId == channel.FirstNodeId && !isSuccess
                || message.LastTransferNodeId != channel.FirstNodeId && isSuccess)
            {
                node = _network.GetNodeById(channel.FirstNodeId);
            }
            else
            {
                node = _network.GetNodeById(channel.SecondNodeId);
            }

            if (!isSuccess)
            {
                message.SendAttempts++;
            }
            else
            {
                message.SendAttempts = 0;
            }

            var messageQueueHandler = node.MessageQueueHandlers
                .First(m => m.ChannelId == channel.Id);

            messageQueueHandler.AddMessageInStart(message);
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
                && channel.FirstMessage?.LastTransferNodeId != message.LastTransferNodeId 
                && channel.SecondMessage?.LastTransferNodeId != message.LastTransferNodeId)
            {
                if (channel.FirstMessage == null)
                {
                    channel.FirstMessage = message;
                }
                else if (channel.SecondMessage == null)
                {
                    channel.SecondMessage = message;
                }
                else
                {
                    return false;
                }
                return true;
            }

            return false;
        }

        private MessageInitializer CreateInitializeMessage(uint senderId, uint receiverId)
        {
            return new MessageInitializer
            {
                MessageType = MessageType.InitializeMessage,
                ReceiverId = receiverId,
                SenderId = senderId,
                Data = null,
                Size = AllConstants.InitializeMessageSize
            };
        }
    }
}

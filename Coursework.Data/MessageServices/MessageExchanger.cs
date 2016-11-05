using System;
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
        private List<Message> _handledMessagesInNode;

        public MessageExchanger(INetworkHandler network, IMessageReceiver messageReceiver)
        {
            _network = network;
            _messageReceiver = messageReceiver;
            _handledMessagesInNode = new List<Message>();
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

                    messageQueueHandler.RemoveMessage(currentMessage);

                    if (!isSuccess)
                    {
                        messageQueueHandler.AppendMessage(currentMessage);
                    }
                    else
                    {
                        _handledMessagesInNode.Add(currentMessage);
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
                var outdatedMessages = messageQueueHandler.Messages
                    .Where(message => message.MessageType == MessageType.MatrixUpdateMessage)
                    .Where(message => _network.GetNodeById(message.ReceiverId).IsTableUpdated);

                foreach (var message in outdatedMessages)
                {
                    message.IsCanceled = true;
                    messageQueueHandler.RemoveMessage(message);
                    node.CanceledMessages.Add(message);
                }
            }
        }

        private void ReplaceMessageToQueue(Channel channel, Message message)
        {
            Node node;

            var isSuccess = AllConstants.RandomGenerator.NextDouble() >= channel.ErrorChance;

            if (channel.IsBusy && channel.MessageOwnerId != message.ParentId)
            {
                isSuccess = false;
            }

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
                ChangeChannelOccupation(message);
            }

            var messageQueueHandler = node.MessageQueueHandlers
                .First(m => m.ChannelId == channel.Id);

            messageQueueHandler.AddMessageInStart(message);
        }

        private bool TryMoveMessageToChannel(Channel channel, Message message)
        {
            if (channel.IsBusy && channel.MessageOwnerId != message.ParentId)
            {
                return false;
            }

            if (channel.ConnectionType == ConnectionType.HalfDuplex && channel.FirstMessage == null)
            {
                channel.FirstMessage = message;
                return true;
            }

            if (channel.ConnectionType != ConnectionType.Duplex
                || channel.FirstMessage?.LastTransferNodeId == message.LastTransferNodeId
                || channel.SecondMessage?.LastTransferNodeId == message.LastTransferNodeId)
            {
                return false;
            }

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

        private void ChangeChannelOccupation(Message message)
        {
            var channel = message.Route.First();

            switch (message.MessageType)
            {
                case MessageType.General:
                    {
                        channel.IsBusy = false;

                        break;
                    }
                case MessageType.SendingRequest:
                    {
                        channel.IsBusy = true;
                        channel.MessageOwnerId = message.ParentId;

                        break;
                    }
                case MessageType.MatrixUpdateMessage:
                case MessageType.SendingResponse:
                    {
                        break;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException();
                    }
            }
        }
    }
}

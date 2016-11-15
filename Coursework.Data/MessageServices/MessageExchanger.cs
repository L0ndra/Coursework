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

            int slotReceivedData;

            if (firstMessage != null
                && !_handledMessagesInNode.Contains(firstMessage))
            {
                slotReceivedData = channel.FirstSlotReceivedData;
                HandleChannelMessage(channel, firstMessage, ref slotReceivedData);
                channel.FirstSlotReceivedData = slotReceivedData;
            }

            else if (secondMessage != null
                    && !_handledMessagesInNode.Contains(secondMessage))
            {
                slotReceivedData = channel.SecondSlotReceivedData;
                HandleChannelMessage(channel, secondMessage, ref slotReceivedData);
                channel.SecondSlotReceivedData = slotReceivedData;
            }
        }

        private void HandleChannelMessage(Channel channel, Message message, ref int slotReceivedDataSize)
        {
            slotReceivedDataSize += channel.Capacity;

            if (slotReceivedDataSize < message.Size)
            {
                return;
            }

            slotReceivedDataSize = 0;

            ReplaceMessageToQueue(channel, message);

            if (channel.FirstMessage != null && channel.FirstMessage.ParentId == message.ParentId)
            {
                channel.FirstMessage = null;
            }
            else
            {
                channel.SecondMessage = null;
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
                MakeChannelFree(message);
            }

            var messageQueueHandler = node.MessageQueueHandlers
                .First(m => m.ChannelId == channel.Id);

            messageQueueHandler.AddMessageInStart(message);
        }

        private void HandleMessagesInNodeQueues(Node node)
        {
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
                    ReplaceMessageInChannel(currentChannel, currentMessage, messageQueueHandler);
                }
                else
                {
                    _messageReceiver.HandleReceivedMessage(node, currentMessage);
                    messageQueueHandler.RemoveMessage(currentMessage);
                    _handledMessagesInNode.Add(currentMessage);
                }
            }
        }

        private void ReplaceMessageInChannel(Channel channel, Message message,
            IMessageQueueHandler messageQueueHandler)
        {
            var isSuccess = TryMoveMessageToChannel(channel, message);

            messageQueueHandler.RemoveMessage(message);

            if (!isSuccess)
            {
                if (message.MessageType == MessageType.SendingRequest)
                {
                    var sender = _network.GetNodeById(message.LastTransferNodeId);
                    _messageReceiver.HandleReceivedMessage(sender, message);
                }
                else
                {
                    messageQueueHandler.AppendMessage(message);
                }
            }
            else
            {
                _handledMessagesInNode.Add(message);
            }
        }

        private bool TryMoveMessageToChannel(Channel channel, Message message)
        {
            if (channel.IsBusy && channel.MessageOwnerId != message.ParentId)
            {
                return false;
            }

            if (channel.ConnectionType == ConnectionType.HalfDuplex)
            {
                if (channel.FirstMessage != null)
                {
                    return false;
                }

                MakeChannelBusy(message);
                channel.FirstMessage = message;
                return true;
            }

            if (channel.FirstMessage == null
                && channel.FirstNodeId == message.LastTransferNodeId)
            {
                MakeChannelBusy(message);
                channel.FirstMessage = message;
            }
            else if (channel.SecondMessage == null
                && channel.SecondNodeId == message.LastTransferNodeId)
            {
                MakeChannelBusy(message);
                channel.SecondMessage = message;
            }
            else
            {
                return false;
            }

            return true;
        }

        private void MakeChannelBusy(Message message)
        {
            var channel = message.Route.First();

            if (message.MessageType == MessageType.SendingRequest)
            {
                channel.IsBusy = true;
                channel.MessageOwnerId = message.ParentId;
            }
        }

        private void MakeChannelFree(Message message)
        {
            var channel = message.Route.First();

            if (message.MessageType == MessageType.General
                || message.MessageType == MessageType.NegativeSendingResponse
                || message.MessageType == MessageType.MatrixUpdateMessage)
            {
                channel.IsBusy = false;
            }
        }
    }
}

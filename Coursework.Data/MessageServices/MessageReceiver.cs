using System;
using System.Linq;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;

namespace Coursework.Data.MessageServices
{
    public class MessageReceiver : IMessageReceiver
    {
        private readonly INetworkHandler _network;
        private readonly IMessageCreator _messageCreator;

        public MessageReceiver(INetworkHandler network, IMessageCreator messageCreator)
        {
            _network = network;
            _messageCreator = messageCreator;
        }

        public void HandleReceivedMessage(Node node, Message message)
        {
            message.LastTransferNodeId = node.Id;

            message.Route = message.Route
                .Skip(1)
                .ToArray();

            if (message.Route.Length != 0)
            {
                var destinationMessageQueue = node.MessageQueueHandlers
                    .First(m => m.ChannelId == message.Route[0].Id);

                destinationMessageQueue.AppendMessage(message);
            }
            else
            {
                if (message.MessageType == MessageType.InitializeMessage)
                {
                    node.IsActive = true;
                    InitializeLinkedNodes(node);
                }
            }
        }

        private void InitializeLinkedNodes(Node node)
        {
            foreach (var linkedNodeId in node.LinkedNodesId)
            {
                var linkedNode = _network.GetNodeById(linkedNodeId);

                if (!linkedNode.IsActive)
                {
                    var initializeMessage = CreateInitializeMessage(node.Id, linkedNodeId);
                    _messageCreator.AddInQueue(new[] { initializeMessage });
                }
            }
        }

        private Message CreateInitializeMessage(uint senderId, uint receiverId)
        {
            var channel = _network.GetChannel(senderId, receiverId);

            return new Message
            {
                MessageType = MessageType.InitializeMessage,
                ReceiverId = receiverId,
                SenderId = senderId,
                Data = null,
                Size = AllConstants.InitializeMessageSize,
                LastTransferNodeId = senderId,
                Route = new[] { channel },
                ParentId = Guid.NewGuid(),
                SendAttempts = 0
            };
        }
    }
}

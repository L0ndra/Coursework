using System;
using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;

namespace Coursework.Data.MessageServices
{
    public class MessageHandler : IMessageHandler
    {
        private readonly INetworkHandler _network;

        public MessageHandler(INetworkHandler network)
        {
            _network = network;
        }

        public void HandleMessage(Message message)
        {
            switch (message.MessageType)
            {
                case MessageType.General:
                    {
                        _network.RemoveFromQueue(message, message.ReceiverId);
                        break;
                    }
                case MessageType.MatrixUpdateMessage:
                    {
                        HandleUpdateTableMessage(message);
                        break;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException();
                    }
            }
        }

        private void HandleUpdateTableMessage(Message message)
        {
            var currentNode = _network.GetNodeById(message.ReceiverId);

            var networkMatrises = (IDictionary<uint, NetworkMatrix>)message.Data;

            currentNode.IsActive = true;
            currentNode.IsTableUpdated = true;
            currentNode.NetworkMatrix = networkMatrises[currentNode.Id];

            InitializeLinkedNodes(currentNode, networkMatrises);
        }

        private void InitializeLinkedNodes(Node node, IDictionary<uint, NetworkMatrix> networkMatrises)
        {
            foreach (var linkedNodeId in node.LinkedNodesId)
            {
                var linkedNode = _network.GetNodeById(linkedNodeId);

                if (!linkedNode.IsTableUpdated)
                {
                    var initializeMessage = CreateInitializeMessage(node.Id, linkedNodeId, networkMatrises);

                    var channel = initializeMessage.Route.First();

                    var messageQueue = node.MessageQueueHandlers
                        .First(m => m.ChannelId == channel.Id);

                    messageQueue.AddMessageInStart(initializeMessage);
                }
            }
        }

        private Message CreateInitializeMessage(uint senderId, uint receiverId,
            IDictionary<uint, NetworkMatrix> networkMatrses)
        {
            var channel = _network.GetChannel(senderId, receiverId);

            return new Message
            {
                MessageType = MessageType.MatrixUpdateMessage,
                ReceiverId = receiverId,
                SenderId = senderId,
                Data = networkMatrses,
                Size = AllConstants.InitializeMessageSize,
                LastTransferNodeId = senderId,
                Route = new[] { channel },
                ParentId = Guid.NewGuid(),
                SendAttempts = 0
            };
        }
    }
}

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
            var receiver = _network.GetNodeById(message.ReceiverId);
            receiver.ReceivedMessages.Add(message);

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
                case MessageType.SendingRequest:
                    {
                        HandleSendingRequest(message);
                        break;
                    }
                case MessageType.SendingResponse:
                    {
                        HandleSendingResponse(message);
                        break;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException();
                    }
            }

            message.Route = message.Route
                    .Skip(1)
                    .ToArray();
        }

        private void HandleSendingResponse(Message response)
        {
            var messages = (Message[])response.Data;

            var currentNode = _network.GetNodeById(response.ReceiverId);

            var lastChannel = response.Route.First();

            var messageQueue = currentNode.MessageQueueHandlers
                .First(m => m.ChannelId == lastChannel.Id);

            foreach (var message in messages)
            {
                messageQueue.AppendMessage(message);
            }

            messageQueue.RemoveMessage(response);
        }

        private void HandleSendingRequest(Message request)
        {
            var response = CreateResponseMessage(request);

            var currentNode = _network.GetNodeById(request.ReceiverId);

            var lastChannel = response.Route.First();

            var messageQueue = currentNode.MessageQueueHandlers
                .First(m => m.ChannelId == lastChannel.Id);

            messageQueue.AppendMessage(response);
            messageQueue.RemoveMessage(request);
        }

        private void HandleUpdateTableMessage(Message message)
        {
            var currentNode = _network.GetNodeById(message.ReceiverId);

            var networkMatrises = (IDictionary<uint, NetworkMatrix>)message.Data;

            currentNode.IsActive = true;
            currentNode.IsTableUpdated = true;
            currentNode.NetworkMatrix = networkMatrises[currentNode.Id];

            RemoveInitializeMessages(currentNode);
            InitializeLinkedNodes(currentNode, networkMatrises);
        }

        private void RemoveInitializeMessages(Node node)
        {
            foreach (var messageQueueHandler in node.MessageQueueHandlers)
            {
                var outdatedMessages = messageQueueHandler.Messages
                    .Where(m => m.MessageType == MessageType.MatrixUpdateMessage);

                foreach (var message in outdatedMessages)
                {
                    messageQueueHandler.RemoveMessage(message);
                    node.CanceledMessages.Add(message);
                }
            }
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

        private Message CreateResponseMessage(Message request)
        {
            var dataMessages = (Message[])request.Data;

            var reversedRoute = dataMessages.First()
                .Route
                .Reverse();

            return new Message
            {
                MessageType = MessageType.SendingResponse,
                ReceiverId = request.SenderId,
                SenderId = request.ReceiverId,
                Route = reversedRoute.ToArray(),
                Data = request.Data,
                LastTransferNodeId = request.ReceiverId,
                ParentId = dataMessages.First().ParentId,
                SendAttempts = 0,
                DataSize = 0,
                ServiceSize = AllConstants.ResponseMessageSize
            };
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
                DataSize = 0,
                ServiceSize = AllConstants.InitializeMessageSize,
                LastTransferNodeId = senderId,
                Route = new[] { channel },
                ParentId = Guid.NewGuid(),
                SendAttempts = 0
            };
        }
    }
}

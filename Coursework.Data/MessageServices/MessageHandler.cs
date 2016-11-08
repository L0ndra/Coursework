using System;
using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;

namespace Coursework.Data.MessageServices
{
    public class MessageHandler : IMessageHandler
    {
        private readonly INetworkHandler _network;
        private readonly IMessageCreator _generalMessageCreator;
        private readonly IMessageCreator _updateMatrixMessageCreator;
        private readonly IMessageCreator _positiveResponseMessageCreator;

        public MessageHandler(INetworkHandler network, IMessageCreator generalMessageCreator,
            IMessageCreator updateMatrixMessageCreator,
            IMessageCreator positiveResponseMessageCreator)
        {
            _network = network;
            _generalMessageCreator = generalMessageCreator;
            _updateMatrixMessageCreator = updateMatrixMessageCreator;
            _positiveResponseMessageCreator = positiveResponseMessageCreator;
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
                case MessageType.PositiveSendingResponse:
                    {
                        HandlePositiveSendingResponse(message);
                        break;
                    }
                case MessageType.NegativeSendingResponse:
                    {
                        HandleNegativeSendingResponse(message);
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

        private void HandleNegativeSendingResponse(Message response)
        {
            var oldMessages = (Message[])response.Data;
            var firstMessage = oldMessages.First();

            var sumSize = oldMessages.Sum(oldMessage => oldMessage.DataSize);

            var messageInitializer = new MessageInitializer
            {
                MessageType = firstMessage.MessageType,
                ReceiverId = firstMessage.ReceiverId,
                SenderId = firstMessage.SenderId,
                Size = sumSize
            };

            var messages = _generalMessageCreator.CreateMessages(messageInitializer);
            _generalMessageCreator.AddInQueue(messages, firstMessage.SenderId);

            _network.RemoveFromQueue(response, response.ReceiverId);
        }

        private void HandlePositiveSendingResponse(Message response)
        {
            var messages = (Message[])response.Data;

            foreach (var message in messages)
            {
                _network.AddInQueue(message, response.ReceiverId);
            }

            _network.RemoveFromQueue(response, response.ReceiverId);
        }

        private void HandleSendingRequest(Message request)
        {
            var responseInitializer = new MessageInitializer
            {
                ReceiverId = request.SenderId,
                SenderId = request.ReceiverId,
                MessageType = MessageType.PositiveSendingResponse,
                Data = request.Data
            };

            var responses = _positiveResponseMessageCreator.CreateMessages(responseInitializer);

            foreach (var response in responses)
            {
                response.ParentId = request.ParentId;
            }

            _positiveResponseMessageCreator.AddInQueue(responses, request.ReceiverId);
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
                    message.IsCanceled = true;
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

                if (linkedNode.IsTableUpdated)
                {
                    continue;
                }

                var messageInitializer = new MessageInitializer
                {
                    Data = networkMatrises,
                    MessageType = MessageType.MatrixUpdateMessage,
                    ReceiverId = linkedNodeId,
                    SenderId = node.Id,
                };

                var initializeMessage = _updateMatrixMessageCreator.CreateMessages(messageInitializer);

                _updateMatrixMessageCreator.AddInQueue(initializeMessage, node.Id);
            }
        }
    }
}

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
        private readonly IMessageCreator _messageCreator;
        private readonly IMessageRouter _messageRouter;

        public MessageHandler(INetworkHandler network, IMessageCreator messageCreator, IMessageRouter messageRouter)
        {
            _network = network;
            _messageCreator = messageCreator;
            _messageRouter = messageRouter;
        }

        public void HandleMessage(Message message)
        {
            if (message.MessageType == MessageType.InitializeMessage)
            {
                HandleInitializeMessage(message);
            }
            else if (message.MessageType == MessageType.SendingRequest)
            {
                HandleSendingRequest(message);
            }
            else if (message.MessageType == MessageType.SendingResponse)
            {
                HandleSendingResponse(message);
            }
        }

        private void HandleInitializeMessage(Message message)
        {
            var currentNode = _network.GetNodeById(message.ReceiverId);

            currentNode.IsActive = true;
            InitializeLinkedNodes(currentNode);
        }

        private void HandleSendingResponse(Message message)
        {
            var messages = message.Data as Message[];

            _messageCreator.AddInQueue(messages);
        }

        private void HandleSendingRequest(Message currentMessage)
        {
            var requestReceiver = _network.GetNodeById(currentMessage.ReceiverId);

            if (requestReceiver.NodeType == NodeType.CentralMachine)
            {
                var messages = (Message[])currentMessage.Data;

                if (!TryRebuildRoutes(messages))
                {
                    return;
                }

                var responseInitializer = CreateResponseMessage(messages, requestReceiver.Id);

                var responseMessage = _messageCreator.CreateMessages(responseInitializer);

                if (responseMessage != null)
                {
                    _messageCreator.AddInQueue(responseMessage
                        .Where(m => m.SenderId != m.ReceiverId)
                        .ToArray());

                    foreach (var message in responseMessage.Where(m => m.SenderId == m.ReceiverId))
                    {
                        HandleMessage(message);
                    }
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

        private bool TryRebuildRoutes(Message[] messages)
        {
            foreach (var message in messages)
            {
                var route = _messageRouter.GetRoute(message.SenderId, message.ReceiverId);

                if (route == null)
                {
                    _messageCreator.RemoveFromQueue(messages);
                    return false;
                }

                message.Route = route;

                _messageCreator.AddInQueue(new[] { message });
            }

            _messageCreator.RemoveFromQueue(messages);

            return true;
        }

        private MessageInitializer CreateResponseMessage(Message[] messages, uint centralMachineId)
        {
            return new MessageInitializer
            {
                MessageType = MessageType.SendingResponse,
                ReceiverId = messages.First().SenderId,
                SenderId = centralMachineId,
                Data = messages,
                Size = AllConstants.SendingResponseMessageSize,
            };
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

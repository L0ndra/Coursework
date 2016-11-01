﻿using System.Linq;
using Coursework.Data.Entities;

namespace Coursework.Data.MessageServices
{
    public class MessageReceiver : IMessageReceiver
    {
        private readonly IMessageHandler _messageHandler;

        public MessageReceiver(IMessageHandler messageHandler)
        {
            _messageHandler = messageHandler;
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
                _messageHandler.HandleMessage(message);
            }
        }
    }
}

﻿using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;

namespace Coursework.Data.MessageServices
{
    public class PackageMessageCreator : MessageCreator
    {
        public PackageMessageCreator(INetworkHandler network, IMessageRouter messageRouter)
            : base(network, messageRouter)
        {
        }

        public override Message[] CreateMessages(MessageInitializer messageInitializer)
        {
            var undividedMessages = base.CreateMessages(messageInitializer);

            if (undividedMessages != null)
            {
                return DivideIntoPackages(undividedMessages.First());
            }

            return null;
        }

        private Message[] DivideIntoPackages(Message currentMessage)
        {
            var messageCount = currentMessage.DataSize / AllConstants.PackageSize
                + (currentMessage.DataSize % AllConstants.PackageSize == 0 ? 0 : 1);

            var messages = new List<Message>();

            for (var i = 0; i < messageCount; i++)
            {
                var route = MessageRouter.GetRoute(currentMessage.SenderId, currentMessage.ReceiverId);

                if (route == null)
                {
                    RemoveFromQueue(messages.ToArray(), currentMessage.SenderId);
                    return null;
                }

                var message = CreateMessage(currentMessage, route, i);
                message.ServiceSize = AllConstants.ServicePartSize;

                if (i == messageCount - 1
                    && currentMessage.DataSize % AllConstants.PackageSize != 0)
                {
                    message.DataSize = currentMessage.DataSize % AllConstants.PackageSize;
                }

                messages.Add(message);

                if (message.Route.Length != 0)
                {
                    AddInQueue(new[] { message }, currentMessage.SenderId);
                }
            }

            RemoveFromQueue(messages.Where(m => m.Route.Length != 0)
                .ToArray(), currentMessage.SenderId);
            return messages.ToArray();
        }

        private Message CreateMessage(Message currentMessage, Channel[] route, int number)
        {
            return new Message
            {
                Data = currentMessage.Data,
                LastTransferNodeId = currentMessage.LastTransferNodeId,
                MessageType = currentMessage.MessageType,
                ParentId = currentMessage.ParentId,
                ReceiverId = currentMessage.ReceiverId,
                SendAttempts = currentMessage.SendAttempts,
                SenderId = currentMessage.SenderId,
                DataSize = AllConstants.PackageSize,
                ServiceSize = AllConstants.ServicePartSize,
                Route = route,
                NumberInPackage = number
            };
        }
    }
}

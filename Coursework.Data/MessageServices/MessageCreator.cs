using System;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;

namespace Coursework.Data.MessageServices
{
    public class MessageCreator : IMessageCreator
    {
        protected readonly INetworkHandler Network;
        protected readonly IMessageRouter MessageRouter;

        public MessageCreator(INetworkHandler network, IMessageRouter messageRouter)
        {
            Network = network;
            MessageRouter = messageRouter;
        }

        public virtual Message[] CreateMessages(MessageInitializer messageInitializer)
        {
            var route = MessageRouter.GetRoute(messageInitializer.SenderId,
                messageInitializer.ReceiverId);

            if (route == null)
            {
                return null;
            }

            var message = new Message
            {
                MessageType = messageInitializer.MessageType,
                ReceiverId = messageInitializer.ReceiverId,
                LastTransferNodeId = messageInitializer.SenderId,
                Route = route,
                SenderId = messageInitializer.SenderId,
                Data = messageInitializer.Data,
                Size = messageInitializer.Size,
                ParentId = Guid.NewGuid(),
                SendAttempts = 0
            };

            return new[] { message };
        }

        public void AddInQueue(Message[] messages)
        {
            foreach (var message in messages)
            {
                Network.AddInQueue(message);
            }
        }

        public void RemoveFromQueue(Message[] messages)
        {
            foreach (var message in messages)
            {
                Network.RemoveFromQueue(message);
            }
        }
    }
}

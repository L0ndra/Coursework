using System.Collections.Generic;
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
            var messageCount = currentMessage.Size / AllConstants.PackageSize
                + (currentMessage.Size % AllConstants.PackageSize == 0 ? 0 : 1);

            var messages = new List<Message>();

            for (var i = 0; i < messageCount; i++)
            {
                var route = MessageRouter.GetRoute(currentMessage.SenderId, currentMessage.ReceiverId);

                if (route == null)
                {
                    RemoveFromQueue(messages.ToArray());
                    return null;
                }

                var message = new Message
                {
                    Data = null,
                    LastTransferNodeId = currentMessage.LastTransferNodeId,
                    MessageType = currentMessage.MessageType,
                    ParentId = currentMessage.ParentId,
                    ReceiverId = currentMessage.ReceiverId,
                    SendAttempts = currentMessage.SendAttempts,
                    SenderId = currentMessage.SenderId,
                    Size = AllConstants.PackageSize + AllConstants.InformationPartSize,
                    Route = route
                };

                if (i == messageCount - 1
                    && currentMessage.Size % AllConstants.PackageSize != 0)
                {
                    message.Size = currentMessage.Size % AllConstants.PackageSize
                                   + AllConstants.InformationPartSize;
                }

                messages.Add(message);

                AddInQueue(new[] { message });
            }

            RemoveFromQueue(messages.ToArray());
            return messages.ToArray();
        }
    }
}

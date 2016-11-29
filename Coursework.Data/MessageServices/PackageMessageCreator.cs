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
            var messageSize = currentMessage.Size - AllConstants.ServicePartSize;

            var messageCount = messageSize / AllConstants.PackageSize
                + (messageSize % AllConstants.PackageSize == 0 ? 0 : 1);

            var messages = new List<Message>();

            for (var i = 0; i < messageCount; i++)
            {
                var route = MessageRouter.GetRoute(currentMessage.SenderId, currentMessage.ReceiverId);

                if (route == null)
                {
                    RemoveFromQueue(messages.ToArray(), currentMessage.SenderId);
                    return null;
                }

                var message = CreateMessage(currentMessage, route, i, messageCount);

                if (i == messageCount - 1
                    && messageSize % AllConstants.PackageSize != 0)
                {
                    if (currentMessage.MessageType == MessageType.General)
                    {
                        message.DataSize = currentMessage.DataSize % AllConstants.PackageSize;
                    }
                    else
                    {
                        message.ServiceSize = (currentMessage.ServiceSize - AllConstants.ServicePartSize) 
                            % AllConstants.PackageSize 
                            + AllConstants.ServicePartSize;
                    }
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

        private int GetDataSize(Message messageInitializer)
        {
            if (messageInitializer.MessageType == MessageType.General)
            {
                return AllConstants.PackageSize;
            }

            return 0;
        }

        private int GetServiceSize(Message messageInitializer)
        {
            if (messageInitializer.MessageType == MessageType.MatrixUpdateMessage
                || messageInitializer.MessageType == MessageType.NegativeSendingResponse
                || messageInitializer.MessageType == MessageType.PositiveSendingResponse
                || messageInitializer.MessageType == MessageType.SendingRequest
                || messageInitializer.MessageType == MessageType.PositiveReceiveResponse)
            {
                return AllConstants.PackageSize;
            }

            return 0;
        }

        private Message CreateMessage(Message currentMessage, Channel[] route, int number,
            int packagesCount)
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
                DataSize = GetDataSize(currentMessage),
                ServiceSize = GetServiceSize(currentMessage) + AllConstants.ServicePartSize,
                Route = route,
                NumberInPackage = number,
                PackagesCount = packagesCount
            };
        }
    }
}

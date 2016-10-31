using System.Collections.Generic;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;

namespace Coursework.Data.MessageServices
{
    public class PackageMessageGenerator : MessageGenerator
    {
        public PackageMessageGenerator(INetworkHandler network, IMessageRouter messageRouter,
            double messageGenerateChance) : base(network, messageRouter, messageGenerateChance)
        {
        }

        protected override void TryAddInQueueRandomMessage(Node sender, Node receiver)
        {
            var undividedMessage = TryCreateMessage(sender, receiver);

            if (undividedMessage != null)
            {
                TryAddInQueuePackages(undividedMessage);

                LastGeneratedMessage = undividedMessage;
            }
        }

        private void TryAddInQueuePackages(Message undividedMessage)
        {
            var messages = DivideIntoPackages(undividedMessage);

            var isSuccess = AddPackageMessagesInQueue(messages);

            if (!isSuccess)
            {
                RemoveMessagesFromQueue(messages);
            }
        }

        private bool AddPackageMessagesInQueue(IEnumerable<Message> messages)
        {
            foreach (var message in messages)
            {
                message.Route = MessageRouter.GetRoute(message.SenderId, message.ReceiverId);

                if (message.Route == null || message.Route.Length == 0)
                {
                    return false;
                }

                Network.AddInQueue(message);
            }

            return true;
        }

        private void RemoveMessagesFromQueue(IEnumerable<Message> messages)
        {
            foreach (var message in messages)
            {
                Network.RemoveFromQueue(message);
            }
        }

        private Message[] DivideIntoPackages(Message currentMessage)
        {
            var messageCount = currentMessage.Size / AllConstants.PackageSize
                + (currentMessage.Size % AllConstants.PackageSize == 0 ? 0 : 1);

            var messages = new List<Message>();

            for (var i = 0; i < messageCount; i++)
            {
                var message = new Message
                {
                    Data = null,
                    LastTransferNodeId = currentMessage.LastTransferNodeId,
                    MessageType = currentMessage.MessageType,
                    ParentId = currentMessage.ParentId,
                    ReceiverId = currentMessage.ReceiverId,
                    SendAttempts = currentMessage.SendAttempts,
                    SenderId = currentMessage.SenderId,
                    Size = AllConstants.PackageSize + AllConstants.InformationPartSize
                };

                if (i == messageCount - 1
                    && currentMessage.Size % AllConstants.PackageSize != 0)
                {
                    message.Size = currentMessage.Size % AllConstants.PackageSize
                                   + AllConstants.InformationPartSize;
                }

                messages.Add(message);
            }

            return messages.ToArray();
        }
    }
}

using System;
using System.Linq;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;

namespace Coursework.Data.MessageServices
{
    public class MessageSender : IMessageSender
    {
        private readonly INetworkHandler _network;

        public MessageSender(INetworkHandler network)
        {
            _network = network;
        }

        public void StartSendProcess(MessageInitializer messageInitializer)
        {
            if (messageInitializer.MessageType == MessageType.InitializeMessage)
            {
                CreateMessage(messageInitializer);
            }
        }

        private void CreateMessage(MessageInitializer messageInitializer)
        {
            var sender = _network.GetNodeById(messageInitializer.SenderId);

            var channel = _network.GetChannel(messageInitializer.ReceiverId, sender.Id);

            var messageQueue = sender.MessageQueueHandlers
                .First(m => m.ChannelId == channel.Id);

            Message message = null;

            if (messageInitializer.MessageType == MessageType.InitializeMessage)
            {
                message = new Message
                {
                    ParentId = Guid.NewGuid(),
                    ReceiverId = messageInitializer.ReceiverId,
                    MessageType = messageInitializer.MessageType,
                    SenderId = sender.Id,
                    LastTransferNodeId = sender.Id,
                    Size = messageInitializer.Size,
                    Data = messageInitializer.Data,
                    Route = new[]
                    {
                        channel
                    }
                };
            }

            messageQueue.AddMessage(message);
        }
    }
}

using System;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;

namespace Coursework.Data.MessageServices
{
    public class UpdateMatrixMessageCreator : MessageCreator
    {
        public UpdateMatrixMessageCreator(INetworkHandler network, IMessageRouter messageRouter) 
            : base(network, messageRouter)
        {
        }

        public override Message[] CreateMessages(MessageInitializer messageInitializer)
        {
            var result = new[] { CreateMessage(messageInitializer) };

            return result;
        }

        private Message CreateMessage(MessageInitializer messageInitializer)
        {
            var channel = Network.GetChannel(messageInitializer.ReceiverId, messageInitializer.SenderId);

            return new Message
            {
                Data = messageInitializer.Data,
                LastTransferNodeId = messageInitializer.SenderId,
                MessageType = MessageType.MatrixUpdateMessage,
                ParentId = Guid.NewGuid(),
                ReceiverId = messageInitializer.ReceiverId,
                SendAttempts = 0,
                SenderId = messageInitializer.SenderId,
                DataSize = 0,
                ServiceSize = AllConstants.InitializeMessageSize,
                Route = new[] { channel },
            };
        }
    }
}

using System;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;

namespace Coursework.Data.MessageServices
{
    public class SpecifiedSizeMessageGenerator : MessageGenerator
    {
        private int _messageSize;

        public SpecifiedSizeMessageGenerator(INetworkHandler network, IMessageCreator messageCreator, 
            double messageGenerateChance, int messageSize) 
            : base(network, messageCreator, messageGenerateChance)
        {
            if (messageSize <= 0)
            {
                throw new ArgumentException("messageSize");
            }

            _messageSize = messageSize;
        }

        protected override MessageInitializer CreateMessageInitializer(uint senderId, uint receiverId)
        {
            var messageInitializer = new MessageInitializer
            {
                ReceiverId = receiverId,
                MessageType = MessageType.General,
                SenderId = senderId,
                Data = null,
                Size = _messageSize
            };

            return messageInitializer;
        }
    }
}

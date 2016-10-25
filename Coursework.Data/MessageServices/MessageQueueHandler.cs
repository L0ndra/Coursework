using System;
using System.Collections.Generic;
using Coursework.Data.Entities;

namespace Coursework.Data.MessageServices
{
    public class MessageQueueHandler : IMessageQueueHandler
    {
        public int MessagesCount => _messages.Count;
        public Guid ChannelId { get; }
        private readonly IList<Message> _messages;
        
        public MessageQueueHandler(Guid channelId)
        {
            ChannelId = channelId;
            _messages = new List<Message>();
        }

        public void AddMessage(Message message)
        {
            _messages.Add(message);
        }
    }
}

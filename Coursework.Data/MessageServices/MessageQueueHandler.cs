using System;
using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Entities;

namespace Coursework.Data.MessageServices
{
    public class MessageQueueHandler : IMessageQueueHandler
    {
        public Guid ChannelId { get; }
        private readonly IList<Message> _messages;
        public int MessagesCount => _messages.Count;
        public Message[] Messages => _messages.ToArray();

        public MessageQueueHandler(Guid channelId)
        {
            ChannelId = channelId;
            _messages = new List<Message>();
        }

        public void AddMessage(Message message)
        {
            _messages.Add(message);
        }

        public void RemoveMessage(Message message)
        {
            _messages.Remove(message);
        }
    }
}

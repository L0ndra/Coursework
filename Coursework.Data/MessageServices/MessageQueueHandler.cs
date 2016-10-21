using System.Collections.Generic;
using Coursework.Data.Entities;

namespace Coursework.Data.MessageServices
{
    public class MessageQueueHandler : IMessageQueueHandler
    {
        public int MessagesCount => _messages.Count;
        private readonly IList<Message> _messages;
        
        public MessageQueueHandler()
        {
            _messages = new List<Message>();
        }

        public void AddMessage(Message message)
        {
            _messages.Add(message);
        }
    }
}

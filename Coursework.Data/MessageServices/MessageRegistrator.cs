using System;
using System.Collections.Generic;
using System.Linq;

namespace Coursework.Data.MessageServices
{
    public class MessageRegistrator : IMessageRegistrator
    {
        private readonly IMessageRepository _messageRepository;
        public long RegisterTact { get; private set; }
        public IDictionary<Guid, long> MessagesStartTimes { get; }
        public IDictionary<Guid, long> MessagesEndTimes { get; }

        public MessageRegistrator(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;

            RegisterTact = 0;
            MessagesStartTimes = new Dictionary<Guid, long>();
            MessagesEndTimes = new Dictionary<Guid, long>();
        }

        public void RegisterMessages()
        {
            var messages = _messageRepository.GetAllMessages()
                .ToArray();

            foreach (var message in messages)
            {
                if (!MessagesStartTimes.Keys.Contains(message.ParentId))
                {
                    MessagesStartTimes[message.ParentId] = RegisterTact;
                }

                if (!MessagesEndTimes.Keys.Contains(message.ParentId)
                    && messages
                        .Where(m => m.ParentId == message.ParentId)
                        .All(m => m.IsReceived || m.IsCanceled))
                {
                    MessagesEndTimes[message.ParentId] = RegisterTact;
                }
            }

            RegisterTact++;
        }
    }
}

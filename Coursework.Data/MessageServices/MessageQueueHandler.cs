using System;
using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Entities;
using Coursework.Data.Exceptions;

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

        public void AppendMessage(Message message)
        {
            ThrowExceptionIfMessageIsIncorrect(message);
            _messages.Add(message);
        }

        public void AddMessageInStart(Message message)
        {
            ThrowExceptionIfMessageIsIncorrect(message);
            _messages.Insert(0, message);
        }

        public void RemoveMessage(Message message)
        {
            _messages.Remove(message);
        }

        private void ThrowExceptionIfMessageIsIncorrect(Message message)
        {
            ThrowExceptionIfSizeIsLessThanZero(message);
            ThrowExceptionIfRouteIsNull(message);
        }

        private static void ThrowExceptionIfRouteIsNull(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.Route == null)
            {
                throw new MessageException("Message route is incorrect");
            }
        }

        private static void ThrowExceptionIfSizeIsLessThanZero(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.DataSize < 0 || message.ServiceSize < 0)
            {
                throw new MessageException("Message size is less or equal than zero");
            }
        }
    }
}

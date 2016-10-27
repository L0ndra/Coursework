using System;
using Coursework.Data.Entities;

namespace Coursework.Data.MessageServices
{
    public interface IMessageQueueHandler
    {
        int MessagesCount { get; }
        Message[] Messages { get; }
        Guid ChannelId { get; }
        void AddMessage(Message message);
    }
}
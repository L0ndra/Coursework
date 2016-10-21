using Coursework.Data.Entities;

namespace Coursework.Data.MessageServices
{
    public interface IMessageQueueHandler
    {
        int MessagesCount { get; }
        void AddMessage(Message message);
    }
}
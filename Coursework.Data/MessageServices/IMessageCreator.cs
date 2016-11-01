using Coursework.Data.Entities;

namespace Coursework.Data.MessageServices
{
    public interface IMessageCreator
    {
        Message[] CreateMessages(MessageInitializer messageInitializer);
        void AddInQueue(Message[] messages);
        void RemoveFromQueue(Message[] messages);
    }
}

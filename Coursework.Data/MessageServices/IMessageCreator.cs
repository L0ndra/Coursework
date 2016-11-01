using Coursework.Data.Entities;

namespace Coursework.Data.MessageServices
{
    public interface IMessageCreator
    {
        void UpdateTables();
        Message[] CreateMessages(MessageInitializer messageInitializer);
        void AddInQueue(Message[] messages, uint nodeId);
        void RemoveFromQueue(Message[] messages, uint nodeId);
    }
}

using Coursework.Data.Entities;

namespace Coursework.Data.MessageServices
{
    public interface IMessageRepository
    {
        Message[] GetAllMessages();
        Message[] GetAllMessages(uint nodeId);
    }
}

using Coursework.Data.Entities;

namespace Coursework.Data.MessageServices
{
    public interface IMessageHandler
    {
        void HandleMessage(Message message);
    }
}

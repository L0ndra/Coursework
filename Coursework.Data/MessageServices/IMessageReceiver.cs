using Coursework.Data.Entities;

namespace Coursework.Data.MessageServices
{
    public interface IMessageReceiver
    {
        void HandleReceivedMessage(Node node, Message message);
    }
}

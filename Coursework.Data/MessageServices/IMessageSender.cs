using Coursework.Data.Entities;

namespace Coursework.Data.MessageServices
{
    public interface IMessageSender
    {
        void StartSendProcess(MessageInitializer messageInitializer);
    }
}

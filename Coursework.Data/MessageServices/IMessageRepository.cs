using Coursework.Data.Entities;

namespace Coursework.Data.MessageServices
{
    public interface IMessageRepository
    {
        Message[] GetAllMessages(uint? nodeId = null,
            MessageFiltrationMode messageFiltrationMode = MessageFiltrationMode.AllMessages);
    }
}

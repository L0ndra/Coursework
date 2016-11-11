using System.Collections.Generic;
using Coursework.Data.Entities;

namespace Coursework.Data.MessageServices
{
    public interface IMessageRepository
    {
        IEnumerable<Message> GetAllMessages(uint? nodeId = null,
            MessageFiltrationMode messageFiltrationMode = MessageFiltrationMode.AllMessages);
    }
}

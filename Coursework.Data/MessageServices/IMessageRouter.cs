using Coursework.Data.Entities;

namespace Coursework.Data.MessageServices
{
    public interface IMessageRouter
    {
        Channel[] GetRoute(uint senderId, uint receiverId);
    }
}

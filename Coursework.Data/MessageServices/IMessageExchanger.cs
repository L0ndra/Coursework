namespace Coursework.Data.MessageServices
{
    public interface IMessageExchanger
    {
        void Initialize();
        void HandleMessagesOnce();
    }
}

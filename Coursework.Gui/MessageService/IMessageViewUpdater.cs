using Coursework.Data.Entities;

namespace Coursework.Gui.MessageService
{
    public interface IMessageViewUpdater
    {
        MessageFiltrationMode MessageFiltrationMode { get; set; }
        void Show();
    }
}

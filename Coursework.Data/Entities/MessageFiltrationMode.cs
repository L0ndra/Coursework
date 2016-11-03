using System;
using System.ComponentModel;

namespace Coursework.Data.Entities
{
    [Flags]
    public enum MessageFiltrationMode
    {
        [Description("No Messages")]
        NoMesages = 0,
        [Description("Received Messages")]
        ReceivedMessagesOnly = 1,
        [Description("Canceled Messages")]
        CanceledMessagesOnly = 2,
        [Description("Messages In Queue")]
        QueueMessagesOnly = 4,
        [Description("Messages In Channel")]
        ChannelMessagesOnly = 8,
        [Description("Active messages")]
        ActiveMessages = QueueMessagesOnly | ChannelMessagesOnly,
        [Description("All Messages")]
        AllMessages = ReceivedMessagesOnly | CanceledMessagesOnly | QueueMessagesOnly | ChannelMessagesOnly
    }
}

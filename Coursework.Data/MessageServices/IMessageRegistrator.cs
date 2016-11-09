using System;
using System.Collections.Generic;

namespace Coursework.Data.MessageServices
{
    public interface IMessageRegistrator
    {
        long RegisterTact { get; }
        IDictionary<Guid, long> MessagesStartTimes { get; } 
        IDictionary<Guid, long> MessagesEndTimes { get; }
        void RegisterMessages();
    }
}

using System;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;

namespace Coursework.Data.MessageServices
{
    public class PositiveResponseCreator : ResponseCreator
    {
        public PositiveResponseCreator(INetworkHandler network, IMessageRouter messageRouter) 
            : base(network, messageRouter)
        {
        }

        protected override void ThrowExceptionIfMessageTypeIsNotCorrectResponse(MessageType responseType)
        {
            if (responseType != MessageType.PositiveSendingResponse)
            {
                throw new ArgumentException("responseType");
            }
        }
    }
}

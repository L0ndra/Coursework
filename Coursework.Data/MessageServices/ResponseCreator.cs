using System;
using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;

namespace Coursework.Data.MessageServices
{
    public abstract class ResponseCreator : MessageCreator
    {
        protected ResponseCreator(INetworkHandler network, IMessageRouter messageRouter) 
            : base(network, messageRouter)
        {
        }

        public override Message[] CreateMessages(MessageInitializer messageInitializer)
        {
            var result = new[] { CreateResponse(messageInitializer) };

            return result;
        }

        protected abstract void ThrowExceptionIfMessageTypeIsNotCorrectResponse(MessageType responseType);

        private Message CreateResponse(MessageInitializer messageInitializer)
        {
            ThrowExceptionIfMessageTypeIsNotCorrectResponse(messageInitializer.MessageType);

            var data = (Message[])messageInitializer.Data;

            IEnumerable<Channel> route = data.First().Route;
            route = route.Reverse();

            return new Message
            {
                Data = messageInitializer.Data,
                LastTransferNodeId = messageInitializer.SenderId,
                MessageType = messageInitializer.MessageType,
                ParentId = Guid.NewGuid(),
                ReceiverId = messageInitializer.ReceiverId,
                SendAttempts = 0,
                SenderId = messageInitializer.SenderId,
                DataSize = 0,
                ServiceSize = AllConstants.InitializeMessageSize,
                Route = route.ToArray(),
            };
        }
    }
}

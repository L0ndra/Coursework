using System;
using System.Linq;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;

namespace Coursework.Data.MessageServices
{
    public class PackageRequestMessageCreator : PackageMessageCreator
    {
        public PackageRequestMessageCreator(INetworkHandler network, IMessageRouter messageRouter)
            : base(network, messageRouter)
        {
        }

        public override Message[] CreateMessages(MessageInitializer messageInitializer)
        {
            var messages = base.CreateMessages(messageInitializer);

            var centralMachine = Network.Nodes
                .FirstOrDefault(n => n.NodeType == NodeType.CentralMachine);

            if (centralMachine != null)
            {
                var route = MessageRouter.GetRoute(messageInitializer.SenderId, centralMachine.Id);

                if (route == null)
                {
                    return null;
                }

                var request = new Message
                {
                    MessageType = MessageType.SendingRequest,
                    ReceiverId = centralMachine.Id,
                    Route = route,
                    SenderId = messageInitializer.SenderId,
                    LastTransferNodeId = messageInitializer.SenderId,
                    Data = messages,
                    Size = AllConstants.SendingRequestMessageSize,
                    ParentId = Guid.NewGuid(),
                    SendAttempts = 0
                };

                return new[] { request };
            }

            return null;
        }
    }
}

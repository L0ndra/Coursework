using System;
using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;

namespace Coursework.Data.MessageServices
{
    public class MessageCreator : IMessageCreator
    {
        protected readonly INetworkHandler Network;
        protected readonly IMessageRouter MessageRouter;

        public MessageCreator(INetworkHandler network, IMessageRouter messageRouter)
        {
            Network = network;
            MessageRouter = messageRouter;
        }

        public void UpdateTables()
        {
            var centralMachine = Network.Nodes
                .FirstOrDefault(n => n.NodeType == NodeType.CentralMachine);

            if (centralMachine != null)
            {
                centralMachine.IsActive = true;

                var networkMatrises = new Dictionary<uint, NetworkMatrix>();

                foreach (var node in Network.Nodes)
                {
                    var networkMatrix = MessageRouter.CountPriceMatrix(node.Id);

                    node.IsTableUpdated = false;
                    networkMatrises[node.Id] = networkMatrix;
                }

                foreach (var linkedNodeId in centralMachine.LinkedNodesId)
                {
                    var initializeMessage = CreateInitializeMessage(centralMachine.Id, linkedNodeId,
                        networkMatrises);

                    var channel = initializeMessage.Route.First();

                    var messageQueue = centralMachine.MessageQueueHandlers
                        .First(m => m.ChannelId == channel.Id);

                    messageQueue.AddMessageInStart(initializeMessage);
                }

                centralMachine.NetworkMatrix = networkMatrises[centralMachine.Id];
                centralMachine.IsTableUpdated = true;
            }
        }

        public virtual Message[] CreateMessages(MessageInitializer messageInitializer)
        {
            var route = MessageRouter.GetRoute(messageInitializer.SenderId,
                messageInitializer.ReceiverId);

            if (route == null)
            {
                return null;
            }

            var message = CreateMessage(messageInitializer, route);

            return new[] { message };
        }

        public void AddInQueue(Message[] messages, uint nodeId)
        {
            foreach (var message in messages)
            {
                Network.AddInQueue(message, nodeId);
            }
        }

        public void RemoveFromQueue(Message[] messages, uint nodeId)
        {
            foreach (var message in messages)
            {
                Network.RemoveFromQueue(message, nodeId);
            }
        }

        private Message CreateMessage(MessageInitializer messageInitializer, Channel[] route)
        {
            return new Message
            {
                MessageType = messageInitializer.MessageType,
                ReceiverId = messageInitializer.ReceiverId,
                LastTransferNodeId = messageInitializer.SenderId,
                Route = route,
                SenderId = messageInitializer.SenderId,
                Data = messageInitializer.Data,
                Size = messageInitializer.Size,
                ParentId = Guid.NewGuid(),
                SendAttempts = 0
            };
        }

        private Message CreateInitializeMessage(uint senderId, uint receiverId,
            IDictionary<uint, NetworkMatrix> networkMatrises)
        {
            var channel = Network.GetChannel(senderId, receiverId);

            return new Message
            {
                MessageType = MessageType.MatrixUpdateMessage,
                ReceiverId = receiverId,
                SenderId = senderId,
                Data = networkMatrises,
                Size = AllConstants.InitializeMessageSize,
                LastTransferNodeId = senderId,
                Route = new[] { channel },
                ParentId = Guid.NewGuid(),
                SendAttempts = 0
            };
        }
    }
}

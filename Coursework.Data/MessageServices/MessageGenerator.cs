using System;
using System.Linq;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;

namespace Coursework.Data.MessageServices
{
    public class MessageGenerator : IMessageGenerator
    {
        public Message LastGeneratedMessage { get; protected set; }
        protected readonly INetworkHandler Network;
        protected readonly IMessageRouter MessageRouter;
        private readonly double _messageGenerateChance;

        public MessageGenerator(INetworkHandler network, IMessageRouter messageRouter,
            double messageGenerateChance)
        {
            if (messageGenerateChance > 1.0 || messageGenerateChance < 0.0)
            {
                throw new ArgumentException("messageGenerateChance");
            }

            _messageGenerateChance = messageGenerateChance;
            Network = network;
            MessageRouter = messageRouter;
        }

        public virtual void Generate()
        {
            if (Network.Nodes.Count(n => n.IsActive) >= 2
                && _messageGenerateChance > AllConstants.RandomGenerator.NextDouble())
            {
                Tuple<Node, Node> senderAndReceiver;

                do
                {
                    senderAndReceiver = ChooseRandomSenderAndReceiver();

                    if (senderAndReceiver.Item1.IsActive && senderAndReceiver.Item2.IsActive)
                    {
                        TryAddInQueueRandomMessage(senderAndReceiver.Item1, senderAndReceiver.Item2);
                    }
                    else
                    {
                        senderAndReceiver = null;
                    }
                } while (senderAndReceiver == null);
            }
        }

        protected virtual void TryAddInQueueRandomMessage(Node sender, Node receiver)
        {
            var message = TryCreateMessage(sender, receiver);

            if (message != null)
            {
                LastGeneratedMessage = message;

                Network.AddInQueue(LastGeneratedMessage);
            }
        }

        protected virtual Message TryCreateMessage(Node sender, Node receiver)
        {
            var messageSize = AllConstants.RandomGenerator.Next(AllConstants.MaxMessageSize) + 1;
            var route = MessageRouter.GetRoute(sender.Id, receiver.Id);

            if (route == null || route.Length == 0)
            {
                return null;
            }

            var message = new Message
            {
                ParentId = Guid.NewGuid(),
                LastTransferNodeId = sender.Id,
                SenderId = sender.Id,
                ReceiverId = receiver.Id,
                MessageType = MessageType.General,
                Data = null,
                Size = messageSize,
                Route = route
            };

            return message;
        }

        private Tuple<Node, Node> ChooseRandomSenderAndReceiver()
        {
            var nodesCount = Network.Nodes.Length;

            var receiver = Network.Nodes
                .ElementAt(AllConstants.RandomGenerator.Next(nodesCount));

            var sender = Network.Nodes
                .ElementAt(AllConstants.RandomGenerator.Next(nodesCount));

            return new Tuple<Node, Node>(sender, receiver);
        }
    }
}

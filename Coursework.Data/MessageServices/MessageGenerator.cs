using System;
using System.Linq;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;

namespace Coursework.Data.MessageServices
{
    public class MessageGenerator : IMessageGenerator
    {
        public Message LastGeneratedMessage { get; private set; }
        private readonly INetworkHandler _network;
        private readonly IMessageRouter _messageRouter;
        private readonly double _messageGenerateChance;

        public MessageGenerator(INetworkHandler network, IMessageRouter messageRouter,
            double messageGenerateChance)
        {
            if (messageGenerateChance > 1.0 || messageGenerateChance < 0.0)
            {
                throw new ArgumentException("messageGenerateChance");
            }

            _messageGenerateChance = messageGenerateChance;
            _network = network;
            _messageRouter = messageRouter;
        }

        public Message Generate()
        {
            if (_messageGenerateChance <= AllConstants.RandomGenerator.NextDouble()
                || _network.Nodes.Count(n => n.IsActive) < 2)
            {
                return null;
            }

            var message = TryCreateRandomMessage();

            if (message == null)
            {
                return null;
            }

            LastGeneratedMessage = message;

            _network.AddInQueue(message);

            return message;
        }

        private Message TryCreateRandomMessage()
        {
            while (true)
            {
                var nodesCount = _network.Nodes.Length;

                var receiver = _network.Nodes
                    .ElementAt(AllConstants.RandomGenerator.Next(nodesCount));

                var sender = _network.Nodes
                    .ElementAt(AllConstants.RandomGenerator.Next(nodesCount));

                if (sender.IsActive && receiver.IsActive)
                {
                    var message = TryCreateMessage(sender, receiver);

                    return message;
                }
            }
        }

        private Message TryCreateMessage(Node sender, Node receiver)
        {
            var messageSize = AllConstants.RandomGenerator.Next(AllConstants.MaxMessageSize);
            var route = _messageRouter.GetRoute(sender.Id, receiver.Id);

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
    }
}

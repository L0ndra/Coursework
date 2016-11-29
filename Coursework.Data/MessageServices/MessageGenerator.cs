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
        private readonly IMessageCreator _messageCreator;
        private readonly double _messageGenerateChance;

        public MessageGenerator(INetworkHandler network, IMessageCreator messageCreator,
            double messageGenerateChance)
        {
            if (messageGenerateChance > 1.0 || messageGenerateChance < 0.0)
            {
                throw new ArgumentException("messageGenerateChance");
            }

            _messageGenerateChance = messageGenerateChance;
            Network = network;
            _messageCreator = messageCreator;
        }

        public virtual void Generate()
        {
            if (Network.Nodes.Count(n => n.IsActive) < 2 ||
                !(_messageGenerateChance > AllConstants.RandomGenerator.NextDouble()))
            {
                return;
            }

            Tuple<Node, Node> senderAndReceiver = null;

            while (senderAndReceiver == null)
            {
                senderAndReceiver = ChooseRandomSenderAndReceiver();

                if (senderAndReceiver.Item1.IsActive && senderAndReceiver.Item2.IsActive
                    && senderAndReceiver.Item1.Id != senderAndReceiver.Item2.Id)
                {
                    CreateRandomMessages(senderAndReceiver.Item1, senderAndReceiver.Item2);
                }
                else
                {
                    senderAndReceiver = null;
                }
            } 
        }

        protected virtual MessageInitializer CreateMessageInitializer(uint senderId, uint receiverId)
        {
            var messageSize = AllConstants.RandomGenerator.Next(AllConstants.MaxMessageSize) + 1;

            var messageInitializer = new MessageInitializer
            {
                ReceiverId = receiverId,
                MessageType = MessageType.General,
                SenderId = senderId,
                Data = null,
                Size = messageSize
            };

            return messageInitializer;
        }

        private void CreateRandomMessages(Node sender, Node receiver)
        {
            var messageInitializer = CreateMessageInitializer(sender.Id, receiver.Id);

            var messages = _messageCreator.CreateMessages(messageInitializer);

            if (messages != null)
            {
                _messageCreator.AddInQueue(messages, messages.First().SenderId);

                LastGeneratedMessage = messages.LastOrDefault();
            }
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

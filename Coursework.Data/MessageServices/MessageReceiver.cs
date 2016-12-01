using System.Linq;
using Coursework.Data.Entities;

namespace Coursework.Data.MessageServices
{
    public class MessageReceiver : IMessageReceiver
    {
        private readonly IMessageHandler _messageHandler;
        private readonly IMessageCreator _negativeResponseMessageCreator;

        public MessageReceiver(IMessageHandler messageHandler,
            IMessageCreator negativeResponseMessageCreator)
        {
            _messageHandler = messageHandler;
            _negativeResponseMessageCreator = negativeResponseMessageCreator;
        }

        public void HandleReceivedMessage(Node node, Message message)
        {
            var passedChannel = message.Route.First();

            var oldMessageQueue = node.MessageQueueHandlers
                    .First(m => m.ChannelId == passedChannel.Id);

            oldMessageQueue.RemoveMessage(message);

            if (message.ReceiverId != node.Id)
            {
                if (message.MessageType == MessageType.SendingRequest
                    && passedChannel.IsBusy && passedChannel.MessageOwnerId != message.ParentId)
                {
                    HandleFailedSendingRequest(node, message);
                }
                else
                {
                    HandleSuccessfullyTransferedMessage(node, message);
                }
            }
            else
            {
                _messageHandler.HandleMessage(message);
            }

            message.LastTransferNodeId = node.Id;
        }

        private static void HandleSuccessfullyTransferedMessage(Node node, Message message)
        {
            message.Route = message.Route
                .Skip(1)
                .ToArray();

            var destinationMessageQueue = node.MessageQueueHandlers
                .First(m => m.ChannelId == message.Route[0].Id);

            destinationMessageQueue.AppendMessage(message);
        }

        private void HandleFailedSendingRequest(Node node, Message request)
        {
            var responseInitializer = CreateNegativeResponseInitializer(node, request);

            var responses = _negativeResponseMessageCreator.CreateMessages(responseInitializer);

            UpdateResponsesByRequest(responses, request);
            
            _negativeResponseMessageCreator.RemoveFromQueue(new[] { request }, request.LastTransferNodeId);
        }

        private void UpdateResponsesByRequest(Message[] responses, Message request)
        {
            foreach (var response in responses)
            {
                response.ParentId = request.ParentId;
                response.Data = new[] { request };

                response.Route = response.Route
                    .Except(request.Route)
                    .ToArray();

                if (response.Route.Length == 0)
                {
                    response.Route = new[] { request.Route.First() };
                    _messageHandler.HandleMessage(response);
                }
                else
                {
                    _negativeResponseMessageCreator.AddInQueue(new[] { response }, request.LastTransferNodeId);
                }
            }
        }

        private static MessageInitializer CreateNegativeResponseInitializer(Node node, Message request)
        {
            return new MessageInitializer
            {
                MessageType = MessageType.NegativeSendingResponse,
                ReceiverId = request.SenderId,
                SenderId = node.Id,
                Data = request.Data
            };
        }
    }
}

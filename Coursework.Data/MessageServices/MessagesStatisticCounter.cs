using System.Linq;
using Coursework.Data.Entities;

namespace Coursework.Data.MessageServices
{
    public class MessagesStatisticCounter : IMessagesStatisticCounter
    {
        private readonly IMessageRegistrator _messageRegistrator;
        private readonly IMessageRepository _messageRepository;

        public MessagesStatisticCounter(IMessageRegistrator messageRegistrator, IMessageRepository messageRepository)
        {
            _messageRegistrator = messageRegistrator;
            _messageRepository = messageRepository;
        }

        public MessagesStatistic Count()
        {
            var allMessages = _messageRepository
                .GetAllMessages()
                .ToArray();

            var receivedMessages = allMessages
                .Where(m => m.IsReceived && allMessages
                    .Where(m1 => m1.ParentId == m.ParentId)
                    .All(m1 => m1.IsReceived || m1.IsCanceled))
                .ToArray();

            var generalReceivedMessages = receivedMessages
                .Where(m => m.MessageType == MessageType.General)
                .ToArray();

            var avarageDeliveryTime = receivedMessages
                .GroupBy(m => m.ParentId)
                .Sum(m => _messageRegistrator.MessagesEndTimes[m.Key]
                            - _messageRegistrator.MessagesStartTimes[m.Key])
                        / (double)receivedMessages.GroupBy(m => m.ParentId).Count();

            var avarageGeneralMessagesDeliveryTime = generalReceivedMessages
                .GroupBy(m => m.ParentId)
                .Sum(m => _messageRegistrator.MessagesEndTimes[m.Key]
                            - _messageRegistrator.MessagesStartTimes[m.Key])
                        / (double)generalReceivedMessages.GroupBy(m => m.ParentId).Count();

            var receivedSize = receivedMessages.Sum(m => m.Size);

            var receivedDataSize = receivedMessages.Sum(m => m.DataSize);

            var messageStatistic = new MessagesStatistic
            {
                MessagesCount = allMessages.GroupBy(m => m.ParentId).Count(),
                ReceivedMessagesCount = receivedMessages.GroupBy(m => m.ParentId).Count(),
                GeneralMessagesReceivedCount = generalReceivedMessages.GroupBy(m => m.ParentId).Count(),
                AvarageDeliveryTime = avarageDeliveryTime,
                AvarageGeneralMessagesDeliveryTime = avarageGeneralMessagesDeliveryTime,
                TotalReceivedMessagesSize = receivedSize,
                TotalReceivedDataSize = receivedDataSize
            };

            return messageStatistic;
        }
    }
}

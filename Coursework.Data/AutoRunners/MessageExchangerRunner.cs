using System.Timers;
using Coursework.Data.Constants;
using Coursework.Data.MessageServices;

namespace Coursework.Data.AutoRunners
{
    public class MessageExchangerRunner : IAutoRunner
    {
        private readonly IMessageExchanger _messageExchanger;
        private Timer _timer;

        public MessageExchangerRunner(IMessageExchanger messageExchanger)
        {
            _messageExchanger = messageExchanger;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _messageExchanger.HandleMessagesOnce();
        }

        public void Run()
        {
            _timer?.Dispose();

            _timer = new Timer(AllConstants.TimerInterval);
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }
    }
}

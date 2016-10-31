using System;
using System.Threading;
using System.Windows.Threading;
using Coursework.Data.AutoRunners;
using Coursework.Data.Constants;
using Coursework.Data.MessageServices;
using Coursework.Gui.Drawers;

namespace Coursework.Gui.Background
{
    public class BackgroundWorker : IBackgroundWorker
    {
        public bool IsActive => _timer != null && _timer.IsEnabled;
        private readonly IMessageExchanger _messageExchanger;
        private readonly IMessageGenerator _messageGenerator;
        private readonly IComponentDrawer _networkDrawer;
        private readonly Mutex _locker;
        private DispatcherTimer _timer;

        public BackgroundWorker(IMessageExchanger messageExchanger, IMessageGenerator messageGenerator,
            IComponentDrawer networkDrawer)
        {
            _messageExchanger = messageExchanger;
            _messageGenerator = messageGenerator;
            _networkDrawer = networkDrawer;
            _locker = new Mutex();
        }

        public void Run()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(AllConstants.TimerInterval),
            };

            _timer.Tick += TimerOnElapsed;
            _timer.IsEnabled = true;

            _timer.Start();
        }

        public void Resume()
        {
            _timer.Tick += TimerOnElapsed;
            _timer.IsEnabled = true;
        }

        public void Pause()
        {
            _timer.Tick -= TimerOnElapsed;
            _timer.IsEnabled = false;
        }

        public void Stop()
        {
            _timer.Tick -= TimerOnElapsed;
            _timer = null;
        }

        private void TimerOnElapsed(object sender, EventArgs e)
        {
            _locker.WaitOne();

            _messageGenerator?.Generate();

            _messageExchanger?.HandleMessagesOnce();

            _networkDrawer?.UpdateComponents();

            _locker.ReleaseMutex();
        }
    }
}

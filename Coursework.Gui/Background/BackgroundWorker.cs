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
        private readonly IMessageCreator _messageCreator;
        private readonly int _updatePeriod;
        private int _accumulator;
        private readonly Mutex _locker;
        private DispatcherTimer _timer;

        public BackgroundWorker(IMessageExchanger messageExchanger, IMessageGenerator messageGenerator,
            IComponentDrawer networkDrawer, IMessageCreator messageCreator, int updatePeriod)
        {
            _messageExchanger = messageExchanger;
            _messageGenerator = messageGenerator;
            _networkDrawer = networkDrawer;
            _messageCreator = messageCreator;
            _updatePeriod = updatePeriod;
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

            _accumulator++;

            if (_accumulator % _updatePeriod == 0)
            {
                _messageCreator.UpdateTables();
            }

            _messageGenerator?.Generate();

            _messageExchanger?.HandleMessagesOnce();

            _networkDrawer?.UpdateComponents();

            _locker.ReleaseMutex();
        }
    }
}

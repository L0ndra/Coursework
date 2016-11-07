using System;
using System.Threading;
using System.Windows.Threading;
using Coursework.Data.AutoRunners;
using Coursework.Data.Constants;
using Coursework.Data.MessageServices;
using Coursework.Gui.Drawers;
using Coursework.Gui.MessageService;

namespace Coursework.Gui.Background
{
    public class BackgroundWorker : IBackgroundWorker, ITimeCounter
    {
        public double Interval
        {
            get
            {
                return _timer?.Interval.TotalMilliseconds ?? AllConstants.TimerInterval;
            }
            set
            {
                if (value <= 0.0)
                {
                    throw new ArgumentException("value");
                }

                if (_timer != null)
                {
                    _timer.Interval = TimeSpan.FromMilliseconds(value);
                }
            }
        }

        public int Ticks { get; private set; }
        private readonly IMessageExchanger _messageExchanger;
        private readonly IMessageGenerator _messageGenerator;
        private readonly IComponentDrawer _networkDrawer;
        private readonly IMessageCreator _messageCreator;
        private readonly int _updatePeriod;
        private readonly IMessageViewUpdater _messageViewUpdated;
        private readonly Mutex _locker;
        private DispatcherTimer _timer;

        public BackgroundWorker(IMessageExchanger messageExchanger, IMessageGenerator messageGenerator,
            IComponentDrawer networkDrawer, IMessageCreator messageCreator,
            IMessageViewUpdater messageViewUpdated, int updatePeriod)
        {
            _messageExchanger = messageExchanger;
            _messageGenerator = messageGenerator;
            _networkDrawer = networkDrawer;
            _messageCreator = messageCreator;
            _updatePeriod = updatePeriod;
            _messageViewUpdated = messageViewUpdated;
            _locker = new Mutex();
        }

        public void Run()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(Interval)
            };

            Ticks = 0;

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

            Ticks++;

            if (Ticks % _updatePeriod == 0)
            {
                _messageCreator.UpdateTables();
            }

            _messageGenerator?.Generate();

            _messageExchanger?.HandleMessagesOnce();

            _networkDrawer?.UpdateComponents();

            _messageViewUpdated.Show();

            _locker.ReleaseMutex();
        }
    }
}

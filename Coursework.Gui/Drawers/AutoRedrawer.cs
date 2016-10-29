using System;
using System.Windows.Threading;
using Coursework.Data.AutoRunners;
using Coursework.Data.Constants;

namespace Coursework.Gui.Drawers
{
    public class AutoRedrawer : IAutoRunner
    {
        private readonly IComponentDrawer _drawer;
        private DispatcherTimer _timer;

        public AutoRedrawer(IComponentDrawer drawer)
        {
            _drawer = drawer;
        }

        private void TimerOnElapsed(object sender, EventArgs e)
        {
            _drawer.UpdateComponents();
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
    }
}

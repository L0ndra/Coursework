using System.Windows;
using Coursework.Data;
using Ninject;

namespace Coursework.Gui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IKernel _container;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ConfigureContainer();
            ComposeObjects();
            Current.MainWindow.Show();
        }

        private void ComposeObjects()
        {
            Current.MainWindow = _container.Get<MainWindow>();
            Current.MainWindow.Title = "Coursework";
        }

        private void ConfigureContainer()
        {
            this._container = new StandardKernel();
            _container.Bind<INetwork>().To<Network>().InTransientScope();
        }
    }
}

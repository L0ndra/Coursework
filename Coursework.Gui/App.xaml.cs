using System;
using System.Windows;
using AutoMapper;
using Coursework.Data;
using Coursework.Data.Builder;
using Coursework.Gui.Drawers;
using Coursework.Gui.Initializers;
using Ninject;

namespace Coursework.Gui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IKernel _container;
        private static uint NodeCount => 5;
        private static double NetworkPower => 2.0;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ConfigureContainer();
            ComposeObjects();
            Mapper.Initialize(MapperInitializer.InitializeMapper);
            Current.MainWindow.Show();
        }

        private void ComposeObjects()
        {
            Current.MainWindow = _container.Get<MainWindow>();
            Current.MainWindow.Title = "Coursework";
        }

        private void ConfigureContainer()
        {
            _container = new StandardKernel();

            _container
                .Bind<Random>()
                .ToSelf()
                .InSingletonScope()
                .WithConstructorArgument("Seed", (int)(DateTime.Now.Ticks & 0xFFFF));

            _container
                .Bind<INetworkBuilder>()
                .To<NetworkBuilder>()
                .InTransientScope()
                .WithConstructorArgument("nodeCount", NodeCount)
                .WithConstructorArgument("networkPower", NetworkPower);

            _container
                .Bind<INetwork>()
                .To<Network>()
                .InTransientScope()
                .Named("Empty");

            _container
                .Bind<INetwork>()
                .ToMethod(x => _container.Get<INetworkBuilder>().Build())
                .InTransientScope()
                .Named("Generated network");

            _container
                .Bind<IComponentDrawer>()
                .To<NodeDrawer>()
                .InTransientScope()
                .Named("Node drawer");

            _container
                .Bind<IComponentDrawer>()
                .To<ChannelDrawer>()
                .InTransientScope()
                .Named("Channel drawer");

            _container
                .Bind<INetworkDrawer>()
                .To<NetworkDrawer>()
                .InTransientScope()
                .WithConstructorArgument("nodeDrawer", _container.Get<IComponentDrawer>("Node drawer"))
                .WithConstructorArgument("channelDrawer", _container.Get<IComponentDrawer>("Channel drawer"));

            _container
                .Bind<MainWindow>()
                .ToSelf()
                .WithConstructorArgument("network", _container.Get<INetwork>("Generated network"))
                .WithConstructorArgument("networkDrawer", _container.Get<INetworkDrawer>());
        }
    }
}

using System;
using System.Windows;
using AutoMapper;
using Coursework.Data;
using Coursework.Data.Builder;
using Coursework.Data.Constants;
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
                .WithConstructorArgument("nodeCount", AllConstants.NodeCount)
                .WithConstructorArgument("networkPower", AllConstants.NetworkPower);

            _container
                .Bind<INetworkHandler>()
                .To<Network>()
                .InTransientScope()
                .Named("Empty");

            _container
                .Bind<INetworkHandler>()
                .ToMethod(x => _container.Get<INetworkBuilder>().Build())
                .InTransientScope()
                .Named("Generated network");

            _container
                .Bind<IComponentDrawer>()
                .To<NodeDrawer>()
                .InTransientScope()
                .Named("Node drawer")
                .WithConstructorArgument("network", _container.Get<INetworkHandler>("Generated network"));

            _container
                .Bind<IComponentDrawer>()
                .To<ChannelDrawer>()
                .InTransientScope()
                .Named("Channel drawer")
                .WithConstructorArgument("network", _container.Get<INetworkHandler>("Generated network"));

            _container
                .Bind<IComponentDrawer>()
                .To<NetworkDrawer>()
                .InTransientScope()
                .Named("Network drawer")
                .WithConstructorArgument("nodeDrawer", _container.Get<IComponentDrawer>("Node drawer"))
                .WithConstructorArgument("channelDrawer", _container.Get<IComponentDrawer>("Channel drawer"));

            _container
                .Bind<MainWindow>()
                .ToSelf()
                .WithConstructorArgument("network", _container.Get<INetworkHandler>("Generated network"));
        }
    }
}

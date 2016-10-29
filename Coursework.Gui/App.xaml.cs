using System;
using System.Windows;
using AutoMapper;
using Coursework.Data;
using Coursework.Data.Builder;
using Coursework.Data.Constants;
using Coursework.Data.IONetwork;
using Coursework.Data.NetworkData;
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
            const string simpleNetworkBuilder = "Simple network builder";
            const string wideAreaNetworkBuilder = "Wide area network builder";
            const string generatedWideAreaNetwork = "Generated wide area network";

            _container = new StandardKernel();

            _container
                .Bind<Random>()
                .ToSelf()
                .InSingletonScope()
                .WithConstructorArgument("Seed", (int)(DateTime.Now.Ticks & 0xFFFF));

            _container
                .Bind<INodeGenerator>()
                .To<NodeGenerator>()
                .InTransientScope();

            _container
                .Bind<INetworkBuilder>()
                .To<NetworkBuilder>()
                .InTransientScope()
                .Named(simpleNetworkBuilder)
                .WithConstructorArgument("nodeCount", AllConstants.NodeCountInMetropolitanNetwork)
                .WithConstructorArgument("networkPower", AllConstants.NetworkPower)
                .WithConstructorArgument("nodeGenerator", _container.Get<INodeGenerator>());

            _container
                .Bind<INetworkBuilder>()
                .To<WideAreaNetworkBuilder>()
                .InTransientScope()
                .Named(wideAreaNetworkBuilder)
                .WithConstructorArgument("simpleNetworkBuilder", _container.Get<INetworkBuilder>(simpleNetworkBuilder))
                .WithConstructorArgument("numberOfMetropolitanNetworks", AllConstants.MetropolitanNetworksCount);

            _container
                .Bind<INetworkHandler>()
                .ToMethod(x => _container.Get<INetworkBuilder>(wideAreaNetworkBuilder).Build())
                .InTransientScope()
                .Named(generatedWideAreaNetwork);

            _container
                .Bind<INetworkInfoRetriever>()
                .To<NetworkInfoRetriever>()
                .InTransientScope();

            _container
                .Bind<MainWindow>()
                .ToSelf()
                .WithConstructorArgument("network", _container.Get<INetworkHandler>(generatedWideAreaNetwork))
                .WithConstructorArgument("networkInfoRetriever", _container.Get<INetworkInfoRetriever>());
        }
    }
}

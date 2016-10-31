using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Coursework.Data.AutoRunners;
using Coursework.Data.Constants;
using Coursework.Data.Entities;
using Coursework.Data.IONetwork;
using Coursework.Data.MessageServices;
using Coursework.Data.NetworkData;
using Coursework.Gui.Dialogs;
using Coursework.Gui.Drawers;
using MahApps.Metro.Controls;
using Microsoft.Win32;

namespace Coursework.Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private INetworkHandler _network;
        private IComponentDrawer _networkDrawer;
        private IComponentDrawer _nodeDrawer;
        private IComponentDrawer _channelDrawer;
        private IMessageExchanger _messageExchanger;
        private IMessageRouter _messageRouter;
        private MessageGenerator _messageGenerator;
        private IBackgroundWorker _backgroundWorker;
        private readonly INetworkInfoRetriever _networkInfoRetriever;
        private readonly ChannelAddWindow _channelAddWindow;
        private Canvas GeneratedCanvas => NetworkArea.Children.OfType<Canvas>().First();

        public MainWindow(INetworkHandler network, INetworkInfoRetriever networkInfoRetriever)
        {
            InitializeComponent();

            _network = network;
            _networkInfoRetriever = networkInfoRetriever;

            InitializeDrawers();
            _channelAddWindow = new ChannelAddWindow(network, channel => _channelDrawer.DrawComponents(GeneratedCanvas));

            Loaded += OnWindowLoaded;
        }

        private void InitializeDrawers()
        {
            _nodeDrawer = new WideAreaNetworkNodeDrawer(_network);
            _channelDrawer = new ChannelDrawer(_network);
            _networkDrawer = new NetworkDrawer(_nodeDrawer, _channelDrawer);
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            _networkDrawer.DrawComponents(NetworkArea);
        }

        private void AddNode_OnClick(object sender, RoutedEventArgs e)
        {
            var node = new Node
            {
                Id = _network.Nodes.Max(n => n.Id) + 1,
                LinkedNodesId = new SortedSet<uint>(),
                MessageQueueHandlers = new List<MessageQueueHandler>(),
                IsActive = false
            };

            _network.AddNode(node);

            _nodeDrawer.DrawComponents(GeneratedCanvas);
        }

        private void AddChannel_OnClick(object sender, RoutedEventArgs e)
        {
            _channelAddWindow.Show();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void SaveNetwork_OnClick(object sender, RoutedEventArgs e)
        {
            var filename = GetPathFromDialog();

            if (filename != null)
            {
                _networkInfoRetriever.Write(filename, _network);

                MessageBox.Show("File saved!", "OK", MessageBoxButton.OK, MessageBoxImage.Information,
                        MessageBoxResult.OK,
                        MessageBoxOptions.None);
            }
        }

        private void ReadNetwork_OnClick(object sender, RoutedEventArgs e)
        {
            var filename = GetPathFromDialog();

            try
            {
                _network = _networkInfoRetriever.Read(filename) as Network;

                InitializeDrawers();
                NetworkArea.Children.Remove(GeneratedCanvas);

                _networkDrawer.DrawComponents(NetworkArea);

                MessageBox.Show("File loaded!", "OK", MessageBoxButton.OK, MessageBoxImage.Information,
                    MessageBoxResult.OK,
                    MessageBoxOptions.None);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.None);
            }
        }

        private string GetPathFromDialog()
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = false,
                Filter = "JSON Files (*.json)|*.json"
            };

            return dialog.ShowDialog() == true
                ? dialog.FileName
                : null;
        }

        private void Start_OnClick(object sender, RoutedEventArgs e)
        {
            if (_backgroundWorker == null)
            {
                InitializeMessageExchanger();

                _messageRouter = new MessageRouter(_network);
                _messageGenerator = new PackageMessageGenerator(_network, _messageRouter, AllConstants.MessageGenerateChance);

                _backgroundWorker = new Background.BackgroundWorker(_messageExchanger, _messageGenerator,
                    _networkDrawer);

                _backgroundWorker.Run();
            }
        }

        private void InitializeMessageExchanger()
        {
            var messageSender = new MessageSender(_network);
            var messageReceiver = new MessageReceiver(_network, messageSender);

            _messageExchanger = new MessageExchanger(_network, messageSender, messageReceiver);

            _messageExchanger.Initialize();

            _networkDrawer.UpdateComponents();
        }

        private void Pause_OnClick(object sender, RoutedEventArgs e)
        {
            if (_backgroundWorker != null && _backgroundWorker.IsActive)
            {
                _backgroundWorker.Pause();
            }
            else
            {
                _backgroundWorker?.Resume();
            }
        }
    }
}

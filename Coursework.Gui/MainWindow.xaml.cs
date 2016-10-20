using System;
using System.Windows;
using System.Windows.Input;
using Coursework.Data;
using Coursework.Gui.Drawers;
using MahApps.Metro.Controls;

namespace Coursework.Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly INetwork _network;
        private readonly IComponentDrawer _networkDrawer;

        public MainWindow(INetwork network, IComponentDrawer networkDrawer)
        {
            InitializeComponent();

            _network = network;
            _networkDrawer = networkDrawer;

            Loaded += OnWindowLoaded;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            var frameworkElement = NetworkArea;

            _networkDrawer.DrawComponents(frameworkElement, _network);
        }
    }
}

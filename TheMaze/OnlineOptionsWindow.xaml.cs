using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TheMaze
{
    /// <summary>
    /// Interaction logic for OnlineOptionsWindow.xaml
    /// </summary>
    public partial class OnlineOptionsWindow : Window
    {
        public OnlineOptionsWindow()
        {
            InitializeComponent();
        }

        private void Host_Click(object sender, RoutedEventArgs e)
        {
            WindowInteraction.AppWindow.ShowMyIP();
            WindowInteraction.AppWindow.HostSequence();
            this.Close();
        }

        private void Connect_Popup_Click(object sender, RoutedEventArgs e)
        {
            connectPopup.IsOpen = true;
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            connectPopup.IsOpen = false;
            WindowInteraction.AppWindow.ClientSequence(hostIPTextBox.Text);
        }
    }
}

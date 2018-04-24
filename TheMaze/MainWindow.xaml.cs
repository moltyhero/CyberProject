using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

// Network namespaces
using System.Net;
using NetworkCommsDotNet;
using NetworkCommsDotNet.DPSBase;
using NetworkCommsDotNet.Tools;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet.Connections.TCP;
using System.Net.Sockets;

namespace TheMaze
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MazeNode playerCurrentLocation; // The current location of the user
        public static string myIP = "My IP is ";
        ScreenOrginizer screenOrginizer;

        public MainWindow()
        {
            InitializeComponent();
            WindowInteraction.AppWindow = this;
            GetLocalIPAddress();
            ipTextBox.Text = myIP;
            //ShowMaze(mazeWindow.Width, mazeWindow.Height, Int32.Parse(mazeRows.Text), Int32.Parse(mazeCols.Text), mainStackPanel);
            
        }

        private void ShowMaze(double width, double height, int rows, int cols, StackPanel stackPanel)
        {
            screenOrginizer = new ScreenOrginizer(width, height, rows, cols);
            screenOrginizer.CreateMaze(stackPanel);
        }
        
        //TODO Maybe respond to the ip I got the object from? Both for Client + Server

        #region Host methods

        // Gets my Local IP and put it in MyIP
        public static void GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    myIP = myIP + ip.ToString();
                }
            }
            //throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public void ShowMyIP()
        {
            ipTextBox.Visibility = Visibility.Visible;
        }

        public void HostSequence ()
        {
            GenerateMaze();
            mainStackPanel.Visibility = Visibility.Hidden;
            generateMazeButton.Visibility = Visibility.Hidden;
            //List<Player> players = new List<Player>();
            // What to do when recieves objects
            lock (screenOrginizer)
            {
                NetworkComms.AppendGlobalIncomingPacketHandler<string>("Joined", (packetHeader, connection, joinedIP) =>
                {
                    foreach (IPEndPoint listenEndPoint in Connection.ExistingLocalListenEndPoints(ConnectionType.TCP))
                    {
                        NetworkComms.SendObject<ScreenOrginizer>("ScreenOrginizer", listenEndPoint.Address.ToString(), listenEndPoint.Port, screenOrginizer);
                    }
                });
            }

            NetworkComms.AppendGlobalIncomingPacketHandler<bool>("GotScreenOrginizer", (packetHeader, connection, incomingApproval) =>
            {
                if (incomingApproval)
                {
                    foreach (IPEndPoint listenEndPoint in Connection.ExistingLocalListenEndPoints(ConnectionType.TCP))
                    {
                        NetworkComms.SendObject<StackPanel>("StackPanel", listenEndPoint.Address.ToString(), listenEndPoint.Port, mainStackPanel);
                    }
                }
                else
                {
                    foreach (IPEndPoint listenEndPoint in Connection.ExistingLocalListenEndPoints(ConnectionType.TCP))
                    {
                        NetworkComms.SendObject<ScreenOrginizer>("ScreenOrginizer", listenEndPoint.Address.ToString(), listenEndPoint.Port, screenOrginizer);
                    }
                }
            });

            // TODO change that to a button that starts the game for multiple connection purposes
            NetworkComms.AppendGlobalIncomingPacketHandler<bool>("GotStackPanel", (packetHeader, connection, incomingApproval) =>
            {
                if (incomingApproval)
                {
                    // Start the game:
                    // Make buttons unavailable
                    // allow movement?
                    foreach (IPEndPoint listenEndPoint in Connection.ExistingLocalListenEndPoints(ConnectionType.TCP))
                    {
                        NetworkComms.SendObject<bool>("StartGame", listenEndPoint.Address.ToString(), listenEndPoint.Port, true);
                    }
                    
                    mainStackPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    foreach (IPEndPoint listenEndPoint in Connection.ExistingLocalListenEndPoints(ConnectionType.TCP))
                    {
                        NetworkComms.SendObject<StackPanel>("StackPanel", listenEndPoint.Address.ToString(), listenEndPoint.Port, mainStackPanel);
                    }
                }
            });

            Connection.StartListening(ConnectionType.TCP, new IPEndPoint(IPAddress.Any, 0));
        }

        #endregion

        #region Client methods

        public void ClientSequence (string hostIP)
        {
            NetworkComms.SendObject<string>("Joined", hostIP, 10000, myIP);

            // What to do when recieves objects
            NetworkComms.AppendGlobalIncomingPacketHandler<ScreenOrginizer>("ScreenOrginizer", (packetHeader, connection, incomingScreenOrginizer) => 
            {
                screenOrginizer = incomingScreenOrginizer;
                NetworkComms.SendObject<bool>("GotScreenOrginizer", hostIP, 10000, true);
            });

            NetworkComms.AppendGlobalIncomingPacketHandler<StackPanel>("StackPanel", (packetHeader, connection, incomingStackPanel) =>
            {
                screenOrginizer.CreateMaze(incomingStackPanel);
                NetworkComms.SendObject<bool>("GotStackPanel", hostIP, 10000, true);
            });

            NetworkComms.AppendGlobalIncomingPacketHandler<bool>("StartGame", (packetHeader, connection, incomingApproval) =>
            {
                HideWhenGameStart();
                // show the maze
                // enable movement
            });


            Connection.StartListening(ConnectionType.TCP, new IPEndPoint(IPAddress.Any, 0));
        }

        #endregion

        #region Window controls management

        // To make sure the user enters numbers only in the maze size textboxes
        private void MazeSizeTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // Hide the controls when the game starts
        private void HideWhenGameStart ()
        {
            mazeSize.Visibility = Visibility.Hidden;
            mazeRows.Visibility = Visibility.Hidden;
            mazeCols.Visibility = Visibility.Hidden;
            generateMazeButton.Visibility = Visibility.Hidden;
        }

        #endregion

        #region Game management

        private void GenerateMaze()
        {
            mainStackPanel.Children.Clear();
            InitializeComponent();
            ShowMaze(mazeWindow.Width, mazeWindow.Height, Int32.Parse(mazeRows.Text), Int32.Parse(mazeCols.Text), mainStackPanel);
            //ScreenOrginizer screenOrginizer = new ScreenOrginizer(mazeWindow.Width, mazeWindow.Height, Int32.Parse(mazeRows.Text), Int32.Parse(mazeCols.Text));
            //screenOrginizer.CreateMaze(mainStackPanel);
            generateMazeButton.IsEnabled = true;
        }

        // Win condition handle
        private void EndPointArrival ()
        {
            if (playerCurrentLocation.Equals(ScreenOrginizer.last))
            {
                generateMazeButton.IsEnabled = false;
                winPopup.IsOpen = true;
            }
        }

        #endregion

        #region Movement

        /// <summary>
        /// Main method for movement. Create a movement illusion for the player. Also verefying wheter the movement is valid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                if (CheckWhetherValidMovement(playerCurrentLocation.Neighbors[MazeNode.West]))
                {
                    playerCurrentLocation.Bounds.Fill = new SolidColorBrush(System.Windows.Media.Colors.White);
                    playerCurrentLocation = playerCurrentLocation.Neighbors[MazeNode.West];
                    playerCurrentLocation.Bounds.Fill = new SolidColorBrush(System.Windows.Media.Colors.Orange);
                }
            }
            else if (e.Key == Key.Right)
            {
                if (CheckWhetherValidMovement(playerCurrentLocation.Neighbors[MazeNode.East]))
                {
                    playerCurrentLocation.Bounds.Fill = new SolidColorBrush(System.Windows.Media.Colors.White);
                    playerCurrentLocation = playerCurrentLocation.Neighbors[MazeNode.East];
                    playerCurrentLocation.Bounds.Fill = new SolidColorBrush(System.Windows.Media.Colors.Orange);
                }
            }
            else  if (e.Key == Key.Up)
            {
                if (CheckWhetherValidMovement(playerCurrentLocation.Neighbors[MazeNode.North]))
                {
                    playerCurrentLocation.Bounds.Fill = new SolidColorBrush(System.Windows.Media.Colors.White);
                    playerCurrentLocation = playerCurrentLocation.Neighbors[MazeNode.North];
                    playerCurrentLocation.Bounds.Fill = new SolidColorBrush(System.Windows.Media.Colors.Orange);
                }
            }
            else if (e.Key == Key.Down)
            {
                if (CheckWhetherValidMovement(playerCurrentLocation.Neighbors[MazeNode.South]))
                {
                    playerCurrentLocation.Bounds.Fill = new SolidColorBrush(System.Windows.Media.Colors.White);
                    playerCurrentLocation = playerCurrentLocation.Neighbors[MazeNode.South];
                    playerCurrentLocation.Bounds.Fill = new SolidColorBrush(System.Windows.Media.Colors.Orange);
                }
            }
            EndPointArrival(); // Handle the win condition.
        }

        private bool CheckWhetherValidMovement (MazeNode goingTo)
        {
            if (goingTo == null)
            {
                return false;
            }
            else if (goingTo.Predecessor == playerCurrentLocation || goingTo == playerCurrentLocation.Predecessor)
            {
                return true;
            }
            return false;
        }

        #endregion

        #region Click events

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            winPopup.IsOpen = false;
            GenerateMaze();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            NetworkComms.Shutdown();
            System.Windows.Application.Current.Shutdown();
        }

        private void Online_Click(object sender, RoutedEventArgs e)
        {
            mainStackPanel.Children.Clear();
            WindowInteraction.onlineOptionsWindow = new OnlineOptionsWindow();
            WindowInteraction.onlineOptionsWindow.Show();
        }

        private void Generate_Maze_Click(object sender, RoutedEventArgs e)
        {
            GenerateMaze();
        }

        #endregion

        
    }
    
    
}

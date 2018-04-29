using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using System.Windows.Threading;

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
        public bool hasFinished = false;
        bool againstTheClockMode = false;
        DispatcherTimer _timer = new DispatcherTimer();
        public static string myIP = "My IP is ";
        public int rowsNum = 0;
        public int colsNum = 0;
        public int currentCols;
        public int currentRows;
        ScreenOrginizer screenOrginizer;

        public MainWindow()
        {
            InitializeComponent();
            WindowInteraction.AppWindow = this;
            GetLocalIPAddress();
            ipTextBlock.Text = myIP;
            currentCols = Int32.Parse(mazeCols.Text);
            currentRows = Int32.Parse(mazeRows.Text);
            //ShowMaze(mazeWindow.Width, mazeWindow.Height, Int32.Parse(mazeRows.Text), Int32.Parse(mazeCols.Text), mainStackPanel);

        }

        private void ShowMaze(double width, double height, int rows, int cols, StackPanel stackPanel, bool shouldFindSpanning, bool shouldDraw)
        {
            screenOrginizer = new ScreenOrginizer(width, height, rows, cols);
            screenOrginizer.CreateMaze(shouldFindSpanning);
            if (shouldDraw)
                screenOrginizer.DrawOnScreen(stackPanel);
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
            ipTextBlock.Visibility = Visibility.Visible;
        }

        public void HostSequence ()
        {
            Connection.StartListening(ConnectionType.TCP, new IPEndPoint(IPAddress.Any, 10000));
            GenerateMaze();
            mainStackPanel.Visibility = Visibility.Hidden;
            generateMazeButton.Visibility = Visibility.Hidden;
            List<Player> players = new List<Player>();
            // What to do when recieves objects
            NetworkComms.AppendGlobalIncomingPacketHandler<string>("Joined", (packetHeader, connection, joinedIP) =>
            {
                foreach (IPEndPoint listenEndPoint in Connection.ExistingLocalListenEndPoints(ConnectionType.TCP))
                {
                    /*mazeRows.Dispatcher.Invoke(() =>
                    {
                        mazeCols.Dispatcher.Invoke(() =>
                        {
                            NetworkComms.SendObject("MazeRows", listenEndPoint.Address.ToString(), listenEndPoint.Port, Int32.Parse(mazeRows.Text));
                            NetworkComms.SendObject("MazeCols", listenEndPoint.Address.ToString(), listenEndPoint.Port, Int32.Parse(mazeCols.Text));
                        });
                    });
                    for (int i = 0; i < colsNum; i++)
                    {
                        for (int j = 0; j < rowsNum; j++)
                        {
                            int row = ScreenOrginizer.Nodes[i, j].Predecessor.row;
                            int col = ScreenOrginizer.Nodes[i, j].Predecessor.col;
                            NetworkComms.SendObject("PredecessorPlace", listenEndPoint.Address.ToString(), listenEndPoint.Port, new Tuple<int, int, int, int>(i, j, col, row));
                        }
                    }
                    NetworkComms.SendObject("HostReady", listenEndPoint.Address.ToString(), listenEndPoint.Port, true);*/
                }
                joinedIP = "192.168.1.19";
                players.Add(new Player(joinedIP));
                mazeRows.Dispatcher.Invoke(() =>
                {
                    mazeCols.Dispatcher.Invoke(() =>
                    {
                        //NetworkComms.SendObject("MazeRows", joinedIP.ToString(), 10000, Int32.Parse(mazeRows.Text));
                        //NetworkComms.SendObject("MazeCols", joinedIP.ToString(), 10000, Int32.Parse(mazeCols.Text));
                        connection.SendObject("MazeRows", Int32.Parse(mazeRows.Text));
                        connection.SendObject("MazeCols", Int32.Parse(mazeCols.Text));
                    });
                });
                for (int i = 0; i < currentCols; i++)
                {
                    for (int j = 0; j < currentRows; j++)
                    {
                        int row = ScreenOrginizer.Nodes[i, j].Predecessor.row;
                        int col = ScreenOrginizer.Nodes[i, j].Predecessor.col;
                        //NetworkComms.SendObject("PredecessorPlace", joinedIP, 10000, new Tuple<int, int, int, int>(i, j, col, row));
                        connection.SendObject("PredecessorPlace", new Tuple<int, int, int, int>(i, j, col, row));
                    }
                }
                //NetworkComms.SendObject("Finish", joinedIP, 10000, true);
                connection.SendObject("Finish", true);

            });

            NetworkComms.AppendGlobalIncomingPacketHandler<bool>("Request", (packetHeader, connection, request) =>
            {
                if (request.Equals("Rows"))
                    //NetworkComms.SendObject("MazeRows", listenEndPoint.Address.ToString(), listenEndPoint.Port, Int32.Parse(mazeRows.Text));
                    connection.SendObject("MazeRows", Int32.Parse(mazeRows.Text));
                else if (request.Equals("Cols"))
                    //NetworkComms.SendObject("MazeCols", listenEndPoint.Address.ToString(), listenEndPoint.Port, Int32.Parse(mazeCols.Text));
                    connection.SendObject("MazeCols", Int32.Parse(mazeCols.Text));
            });

            NetworkComms.AppendGlobalIncomingPacketHandler<Tuple<int,int>>("PredecessorReuquestPlace", (packetHeader, connection, place) =>
            {
                //foreach (IPEndPoint listenEndPoint in Connection.ExistingLocalListenEndPoints(ConnectionType.TCP))
                //{
                    int row = ScreenOrginizer.Nodes[place.Item1, place.Item2].Predecessor.row;
                    int col = ScreenOrginizer.Nodes[place.Item1, place.Item2].Predecessor.col;
                //NetworkComms.SendObject("PredecessorPlace", listenEndPoint.Address.ToString(), listenEndPoint.Port, new Tuple<int,int, int, int>(place.Item1, place.Item2, col, row));
                connection.SendObject("PredecessorPlace", new Tuple<int, int, int, int>(place.Item1, place.Item2, col, row));
                //}
            });


            NetworkComms.AppendGlobalIncomingPacketHandler<bool>("Ready", (packetHeader, connection, ready) =>
            {
                /*foreach (IPEndPoint listenEndPoint in Connection.ExistingLocalListenEndPoints(ConnectionType.TCP))
                {
                    if (ready)
                    {
                        // Start the game:
                        // Make buttons unavailable
                        // allow movement?
                        
                        NetworkComms.SendObject<bool>("StartGame", listenEndPoint.Address.ToString(), listenEndPoint.Port, true);
                    }
                }*/
                if (ready)
                {
                    // Start the game:
                    // Make buttons unavailable
                    // allow movement?

                    //NetworkComms.SendObject<bool>("StartGame", "192.168.1.19", 10000, true);
                    connection.SendObject<bool>("StartGame", true);
                }
                mainStackPanel.Dispatcher.Invoke(() =>
                {
                    mainStackPanel.Visibility = Visibility.Visible;
                });
            });
        }

        #endregion

        #region Client methods

        public void ClientSequence (string hostIP)
        {
            mainStackPanel.Visibility = Visibility.Hidden;
            generateMazeButton.Visibility = Visibility.Hidden;

            NetworkComms.SendObject<string>("Joined", hostIP, 10000, myIP);
            
            NetworkComms.AppendGlobalIncomingPacketHandler<int>("MazeRows", (packetHeader, connection, rows) =>
            {
                this.rowsNum = rows;
            });
            NetworkComms.AppendGlobalIncomingPacketHandler<int>("MazeCols", (packetHeader, connection, cols) =>
            {
                this.colsNum = cols;

                if (colsNum != 0 && rowsNum != 0)
                {
                    mainStackPanel.Dispatcher.Invoke(() =>
                    {
                        mainStackPanel.Children.Clear();
                    });
                    
                    InitializeComponent();
                    mainStackPanel.Dispatcher.Invoke(() =>
                    {
                        ShowMaze(mazeWindow.Width, mazeWindow.Height, rowsNum, colsNum, mainStackPanel, false, false);
                    });
                }
            });

            
            NetworkComms.AppendGlobalIncomingPacketHandler<Tuple<int,int,int,int>>("PredecessorPlace", (packetHeader, connection, place) =>
            {
                ScreenOrginizer.Nodes[place.Item1, place.Item2].Predecessor = ScreenOrginizer.Nodes[place.Item3, place.Item4];
            });

            NetworkComms.AppendGlobalIncomingPacketHandler<bool>("Finish", (packetHeader, connection, ready) =>
            {
                bool hasAll = true;
                if (ready)
                {
                    //Check if have every Predecessor
                    for (int i = 0; i < colsNum; i++)
                    {
                        for (int j = 0; j < rowsNum; j++)
                        {
                            if (ScreenOrginizer.Nodes[i, j].Predecessor == null)
                            {
                                NetworkComms.SendObject("PredecessorReuquestPlace", hostIP, 1000, new Tuple<int, int>(i, j));
                                hasAll = false;
                            }
                            break;
                        }
                    }
                    if (hasAll)
                    {
                        NetworkComms.SendObject("Ready", hostIP, 10000, true);
                    }
                    
                }
                
            });


            NetworkComms.AppendGlobalIncomingPacketHandler<bool>("StartGame", (packetHeader, connection, shouldStart) =>
            {
                if (shouldStart)
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        screenOrginizer.DrawOnScreen(mainStackPanel);
                    });
                    
                    mainStackPanel.Dispatcher.Invoke(() =>
                    {
                        mainStackPanel.Visibility = Visibility.Visible;
                    });
                }
            });

            Connection.StartListening(ConnectionType.TCP, new IPEndPoint(IPAddress.Any, 10000));
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
            //mazeSize.Visibility = Visibility.Hidden;
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
            ShowMaze(mazeWindow.Width, mazeWindow.Height, Int32.Parse(mazeRows.Text), Int32.Parse(mazeCols.Text), mainStackPanel, true, true);
            //ScreenOrginizer screenOrginizer = new ScreenOrginizer(mazeWindow.Width, mazeWindow.Height, Int32.Parse(mazeRows.Text), Int32.Parse(mazeCols.Text));
            //screenOrginizer.CreateMaze(mainStackPanel);
            generateMazeButton.IsEnabled = true;
            if (againstTheClockMode)
            {
                AgainstTheClock();
                _timer.Start();
            }
        }

        // Win condition handle
        private void EndPointArrival ()
        {
            if (playerCurrentLocation.Equals(ScreenOrginizer.last))
            {
                generateMazeButton.IsEnabled = false;
                winPopup.IsOpen = true;
                hasFinished = true;
            }
        }

        private void DrawSolution ()
        {
            if (ScreenOrginizer.last!= null)
            {
                MazeNode node = ScreenOrginizer.last;
                while (node.Predecessor != node)
                {
                    node.Bounds.Fill = new SolidColorBrush(System.Windows.Media.Colors.Orange);
                    node = node.Predecessor;
                }
                node.Bounds.Fill = new SolidColorBrush(System.Windows.Media.Colors.Yellow);
            }
        }
        #endregion

        #region Movement

        #region Movement management
        /// <summary>
        /// Main method for movement. Create a movement illusion for the player. Also verefying wheter the movement is valid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (!hasFinished)
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
                else if (e.Key == Key.Up)
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
            hasFinished = false;
            GenerateMaze();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            NetworkComms.Shutdown();
            System.Windows.Application.Current.Shutdown();
        }

        private void Show_Solution_Click(object sender, RoutedEventArgs e)
        {
            DrawSolution();
        }

        private void Against_The_Clock_Click(object sender, RoutedEventArgs e)
        {
            againstTheClockMode = true;
            modeTextBox.Text = "Against The Clock Mode";
        }

        private void Normal_Mode_Click(object sender, RoutedEventArgs e)
        {
            againstTheClockMode = false;
            modeTextBox.Text = "Normal Mode";
            timer.Visibility = Visibility.Hidden;
            showSolutionButton.Visibility = Visibility.Visible;
        }
        #endregion

        private void AgainstTheClock ()
        {
            showSolutionButton.Visibility = Visibility.Hidden;
            timer.Visibility = Visibility.Visible;

            _timer = new DispatcherTimer();
            TimeSpan _time;

            double timeInSec = Int32.Parse(mazeRows.Text) * Int32.Parse(mazeCols.Text) / 10;
            _time = TimeSpan.FromSeconds(timeInSec);

            _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                timer.Text = _time.ToString("c");
                if (_time == TimeSpan.Zero)
                {
                    wonTextBlock.Text = "You lost!";
                    winPopup.IsOpen = true;
                    hasFinished = true;
                    _timer.Stop();
                }
                _time = _time.Add(TimeSpan.FromSeconds(-1));
            }, Application.Current.Dispatcher);
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

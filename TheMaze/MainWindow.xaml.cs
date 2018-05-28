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
        public bool hasFinished = true;
        bool againstTheClockMode = false;
        DispatcherTimer _timer = new DispatcherTimer();
        public static string myIP = "";
        public int rowsNum = 0;
        public int colsNum = 0;
        public int currentCols;
        public int currentRows;
        ScreenOrginizer screenOrginizer;
        bool onlineMode = false;
        string hostIP;
        List<Player> players;

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

        #region Host methods

        // Gets my Local IP and put it in MyIP
        public static void GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    myIP = ip.ToString();
                }
            }
            //throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public void ShowMyIP()
        {
            ipTextBlock.Visibility = Visibility.Visible;
        }

        public void HideMyIP()
        {
            ipTextBlock.Visibility = Visibility.Hidden;
        }
        public void HostStart()
        {
            GenerateMaze();
            mainStackPanel.Visibility = Visibility.Hidden;
            generateMazeButton.Visibility = Visibility.Hidden;
            onlineMode = true;
            players = new List<Player>();
        }

        public void HostSequence ()
        {
            ShowMyIP();
            try
            {
                Connection.StartListening(ConnectionType.TCP, new IPEndPoint(IPAddress.Any, 10000));
            }
            catch
            { }

            HostStart();

            // What to do when recieves objects
            NetworkComms.AppendGlobalIncomingPacketHandler<string>("Joined", (packetHeader, connection, joinedIP) =>
            {

                players.Add(new Player(joinedIP));

                mazeRows.Dispatcher.Invoke(() =>
                {
                    mazeCols.Dispatcher.Invoke(() =>
                    {
                        connection.SendObject("MazeRows", Int32.Parse(mazeRows.Text));
                        connection.SendObject("MazeCols", Int32.Parse(mazeCols.Text));
                    });
                });
                // To allow the client time to proccess the data
                Thread.Sleep(1000);
                for (int i = 0; i < currentCols; i++)
                {
                    for (int j = 0; j < currentRows; j++)
                    {
                        int row = ScreenOrginizer.Nodes[i, j].Predecessor.row;
                        int col = ScreenOrginizer.Nodes[i, j].Predecessor.col;
                        connection.SendObject("PredecessorPlace", new Tuple<int, int, int, int>(i, j, col, row));
                    }
                }
                connection.SendObject("LastPlace", new Tuple<int, int>(ScreenOrginizer.last.col, ScreenOrginizer.last.row));
                connection.SendObject("Finish", true);

            });

            // General request for lost data
            NetworkComms.AppendGlobalIncomingPacketHandler<bool>("Request", (packetHeader, connection, request) =>
            {
                if (request.Equals("Rows"))
                    connection.SendObject("MazeRows", Int32.Parse(mazeRows.Text));
                else if (request.Equals("Cols"))
                    connection.SendObject("MazeCols", Int32.Parse(mazeCols.Text));
            });

            // In case a player didn't get all the data
            NetworkComms.AppendGlobalIncomingPacketHandler<Tuple<int,int>>("PredecessorReuquestPlace", (packetHeader, connection, place) =>
            {
                int row = ScreenOrginizer.Nodes[place.Item1, place.Item2].Predecessor.row;
                int col = ScreenOrginizer.Nodes[place.Item1, place.Item2].Predecessor.col;
                connection.SendObject("PredecessorPlace", new Tuple<int, int, int, int>(place.Item1, place.Item2, col, row));
            });

            // When a player is ready
            NetworkComms.AppendGlobalIncomingPacketHandler<bool>("Ready", (packetHeader, connection, ready) =>
            {
                if (ready)
                {
                    // Start the game:
                    // Make buttons unavailable
                    // allow movement?

                    connection.SendObject<bool>("StartGame", true);
                }
                mainStackPanel.Dispatcher.Invoke(() =>
                {
                    mainStackPanel.Visibility = Visibility.Visible;
                });
            });

            // Handle win condition
            NetworkComms.AppendGlobalIncomingPacketHandler<bool>("PlayerWon", (packetHeader, connection, won) =>
            {
                EndPointArrival(false);
            });
        }

        #endregion

        #region Client methods

        public void ClientStart ()
        {
            mainStackPanel.Visibility = Visibility.Hidden;
            generateMazeButton.Visibility = Visibility.Hidden;

            mainStackPanel.Children.Clear();
            InitializeComponent();
            hasFinished = false;

            onlineMode = true;
        }

        public void ClientSequence (string host)
        {
            ClientStart();

            this.hostIP = host;

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

            NetworkComms.AppendGlobalIncomingPacketHandler<Tuple<int, int>>("LastPlace", (packetHeader, connection, place) =>
            {
                //ScreenOrginizer.Nodes[place.Item1, place.Item2].Bounds.Fill = new SolidColorBrush(System.Windows.Media.Colors.Yellow);
                ScreenOrginizer.last = ScreenOrginizer.Nodes[place.Item1, place.Item2];
                //ScreenOrginizer.Nodes[0, 0].Bounds.Fill = new SolidColorBrush(System.Windows.Media.Colors.Black);
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
                                connection.SendObject("PredecessorReuquestPlace", new Tuple<int, int>(i, j));
                                hasAll = false;
                            }
                            break;
                        }
                    }
                    if (hasAll)
                    {
                        connection.SendObject("Ready", true);
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
                        WindowInteraction.onlineOptionsWindow.Hide();
                        hasFinished = false;
                    });
                    
                    mainStackPanel.Dispatcher.Invoke(() =>
                    {
                        mainStackPanel.Visibility = Visibility.Visible;
                    });
                }
            });

            NetworkComms.AppendGlobalIncomingPacketHandler<bool>("PlayerWon", (packetHeader, connection, won) =>
            {
                EndPointArrival(false);
            });

            try
            {
                Connection.StartListening(ConnectionType.TCP, new IPEndPoint(IPAddress.Any, 10000));
            }
            catch
            { }
            
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
            hasFinished = false;
            mainStackPanel.Visibility = Visibility.Visible;
            if (againstTheClockMode)
            {
                AgainstTheClock();
                _timer.Start();
            }
        }

        private void ShowMaze(double width, double height, int rows, int cols, StackPanel stackPanel, bool shouldFindSpanning, bool shouldDraw)
        {
            screenOrginizer = new ScreenOrginizer(width, height, rows, cols);
            screenOrginizer.CreateMaze(shouldFindSpanning);
            if (shouldDraw)
                screenOrginizer.DrawOnScreen(stackPanel);
        }

        // Win condition handle
        private void EndPointArrival (bool hasWon)
        {
            if (playerCurrentLocation.Equals(ScreenOrginizer.last))
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    generateMazeButton.IsEnabled = false;
                    wonTextBlock.Text = "You Won!";
                    winPopup.IsOpen = true;
                    hasFinished = true;
                });

                // Share the win to all players connected
                if (onlineMode)
                {
                    if (players != null)
                    {
                        foreach (Player player in players)
                        {
                            NetworkComms.SendObject("PlayerWon", player.IP, 10000, true);
                        }
                    }
                    else
                    {
                        NetworkComms.SendObject("PlayerWon", hostIP, 10000, true);
                    }
                }
            }

            else if (!hasWon)
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    generateMazeButton.IsEnabled = false;
                    wonTextBlock.Text = "You lost!";
                    winPopup.IsOpen = true;
                    hasFinished = true;
                });
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
                EndPointArrival(true); // Handle the win condition.
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
            if (!onlineMode)
            {
                GenerateMaze();
            }
            else
            {
                if (hostIP != null)
                {
                    ClientStart();
                    NetworkComms.SendObject<string>("Joined", hostIP, 10000, myIP);
                }
                else
                {
                    HostStart();
                }
            }
            
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            NetworkComms.Shutdown();
            System.Windows.Application.Current.Shutdown();
        }

        private void Show_Solution_Click(object sender, RoutedEventArgs e)
        {
            if (ScreenOrginizer.last.Predecessor != null)
            {
                DrawSolution();
            }
            
        }

        private void Against_The_Clock_Click(object sender, RoutedEventArgs e)
        {
            againstTheClockMode = true;
            onlineMode = false;
            NetworkComms.Shutdown();
            modeTextBox.Text = "Against The Clock Mode";
            generateMazeButton.Visibility = Visibility.Hidden;
            HideMyIP();
        }

        private void Normal_Mode_Click(object sender, RoutedEventArgs e)
        {
            againstTheClockMode = false;
            onlineMode = false;
            NetworkComms.Shutdown();
            modeTextBox.Text = "Normal Mode";
            timer.Visibility = Visibility.Hidden;
            showSolutionButton.Visibility = Visibility.Visible;
            generateMazeButton.Visibility = Visibility.Visible;
            HideMyIP();
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
                    EndPointArrival(false);
                    hasFinished = true;
                    _timer.Stop();
                }
                _time = _time.Add(TimeSpan.FromSeconds(-1));
            }, Application.Current.Dispatcher);
        }

        
        
    }


}

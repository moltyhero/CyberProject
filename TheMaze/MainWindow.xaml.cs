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

        public MainWindow()
        {
            InitializeComponent();
            ScreenOrginizer screenOrginizer = new ScreenOrginizer(mazeWindow.Width, mazeWindow.Height, Int32.Parse(mazeRows.Text), Int32.Parse(mazeCols.Text));
        }

        // To make sure the user enters numbers only in the maze size textboxes
        private void MazeSizeTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void GenerateMaze()
        {
            mainStackPanel.Children.Clear();
            InitializeComponent();
            ScreenOrginizer screenOrginizer = new ScreenOrginizer(mazeWindow.Width, mazeWindow.Height, Int32.Parse(mazeRows.Text), Int32.Parse(mazeCols.Text));
            screenOrginizer.CreateMaze(mainStackPanel);
            generateMazeButton.IsEnabled = true;
            if (againstTheClockMode)
            {
                AgainstTheClock();
                _timer.Start();
            }
        }

        // Maze regenerate
        private void Generate_Maze_Click(object sender, RoutedEventArgs e)
        {
            GenerateMaze();
        }

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

        #region Button clicks

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            winPopup.IsOpen = false;
            hasFinished = false;
            GenerateMaze();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
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

        
    }


}

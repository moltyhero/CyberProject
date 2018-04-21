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

namespace TheMaze
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MazeNode playerCurrentLocation; // The current location of the user

        public MainWindow()
        {
            InitializeComponent();
            ScreenOrginizer screenOrginizer = new ScreenOrginizer(mazeWindow.Width, mazeWindow.Height, Int32.Parse(mazeRows.Text), Int32.Parse(mazeCols.Text));
            screenOrginizer.CreateMaze(mainStackPanel);
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
                winPopup.IsOpen = true;
            }
        }


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

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            winPopup.IsOpen = false;
            GenerateMaze();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
    
    
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TheMaze
{
    public class ScreenOrginizer
    {
        public static MazeNode last = new MazeNode();
        public double screenHeight;
        public double screenWidth;
        public int rows;
        public int cols;
        public MazeNode[,] nodes; // Array of the maze's nodes
        public static double rectSize;
        

        public ScreenOrginizer(double screenHeight, double screenWidth, int rows, int cols)
        {
            this.screenHeight = screenHeight;
            this.screenWidth = screenWidth;
            this.rows = rows;
            this.cols = cols;
        }

        // Make the network of MazeNodes.
        private void MakeNodes()
        {
            // Make the nodes.
            nodes = new MazeNode[cols, rows];
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    nodes[i, j] = new MazeNode();
                }
            }
            Console.WriteLine(nodes);
            // Initialize the nodes' neighbors.
            for (int c = 0; c < cols; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    if (r > 0)
                        nodes[c, r].Neighbors[MazeNode.North] = nodes[c, r-1];
                    if (r < rows - 1)
                        nodes[c, r].Neighbors[MazeNode.South] = nodes[c, r + 1];
                    if (c > 0)
                        nodes[c, r].Neighbors[MazeNode.West] = nodes[c - 1, r];
                    if (c < cols - 1)
                        nodes[c, r].Neighbors[MazeNode.East] = nodes[c + 1, r];
                }
            }
            MainWindow.playerCurrentLocation = nodes[0, 0];
        }

        // Calculate the desired size of the rectangle
        private void CalcRectSize(double StackHeight, double StackWidth, int rows, int cols)
        {
            double HeightSize;
            double WidthSize;
            HeightSize = StackHeight / rows;
            WidthSize = StackWidth / cols;
            rectSize = Math.Min(HeightSize/1.5, WidthSize/1.5);
        }

        /// <summary>
        /// Calls all the methods that are needed for the maze generation, and puts it on the screen
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        public void CreateMaze(StackPanel main)
        {
            double w = screenWidth * 4 / 6;
            double h = screenHeight * 4 / 6;
            CalcRectSize(h, w, rows, cols);
            MakeNodes();
            Algorithm.FindSpanningTree(nodes[0, 0]);

            // Add stack pannels to the main stack pannel
            for (int i = 0; i < this.cols; i++)
            {
                StackPanel stackPannel = new StackPanel()
                {
                    Name = ("col" + i)
                };

                for (int j = 0; j < this.rows; j++)
                {
                    // Should use the MazeNode.GenerateBounds() method instead of this
                    /*Rectangle rect = new Rectangle()
                    {
                        Height = rectSize,
                        Width = rectSize,
                        Fill = new SolidColorBrush(System.Windows.Media.Colors.Gray),
                        StrokeThickness = 1,
                        Stroke = Brushes.Violet
                    };*/
                    nodes[i, j].GenerateBounds();
                    nodes[i, j].GenerateBorders();
                    stackPannel.Children.Add(nodes[i,j].Borders);
                    nodes[i, j].Borders.Child = nodes[i, j].Bounds;
                }
                main.Children.Add(stackPannel);
            }

        }
    }
}

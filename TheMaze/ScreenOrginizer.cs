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
        double screenHeight;
        double screenWidth;
        int rows;
        int cols;
        public static double rectSize;
        MazeNode[,] nodes; // Array of the maze's nodes

        public ScreenOrginizer(double screenHeight, double screenWidth, int rows, int cols)
        {
            this.screenHeight = screenHeight;
            this.screenWidth = screenWidth;
            this.rows = rows;
            this.cols = cols;
            nodes = new MazeNode[cols, rows];
        }

        // Make the network of MazeNodes.
        private MazeNode[,] MakeNodes()
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

            // Initialize the nodes' neighbors.
            for (int r = 0; r < cols; r++)
            {
                for (int c = 0; c < rows; c++)
                {
                    if (r > 0)
                        nodes[r, c].Neighbors[MazeNode.North] = nodes[r - 1, c];
                    if (r < cols - 1)
                        nodes[r, c].Neighbors[MazeNode.South] = nodes[r + 1, c];
                    if (c > 0)
                        nodes[r, c].Neighbors[MazeNode.West] = nodes[r, c - 1];
                    if (c < rows - 1)
                        nodes[r, c].Neighbors[MazeNode.East] = nodes[r, c + 1];
                }
            }
            // Return the nodes.
            return nodes;
        }

        // Calculate the desired size of the rectangle
        private void CalcRectSize(double StackHeight, double StackWidth, int rows, int cols)
        {
            double HeightSize;
            double WidthSize;
            HeightSize = StackHeight / rows;
            WidthSize = StackWidth / cols;
            rectSize = Math.Min(HeightSize, WidthSize);
        }

        /// <summary>
        /// Puts the maze on the screen, 
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        public void CreateMaze(StackPanel main)
        {
            double w = screenWidth * 4 / 6;
            double h = screenHeight * 4 / 6;
            CalcRectSize(h, w, rows, cols);
            MakeNodes();

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
                    stackPannel.Children.Add(nodes[i, j].Bounds);
                }
                main.Children.Add(stackPannel);
            }

        }
    }
}

using ProtoBuf;
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
    [ProtoContract]
    public class ScreenOrginizer
    {
        public static MazeNode last = new MazeNode();
        public double screenHeight;
        public double screenWidth;
        public int rows;
        public int cols;
        public static MazeNode[,] Nodes { get; private set; } // Array of the maze's nodes
        public static double rectSize;
        
        protected ScreenOrginizer() { }

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
            Nodes = new MazeNode[cols, rows];
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    Nodes[i, j] = new MazeNode();
                    Nodes[i, j].col = i;
                    Nodes[i, j].row = j;
                }
            }
            Console.WriteLine(Nodes);
            // Initialize the nodes' neighbors.
            for (int c = 0; c < cols; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    if (r > 0)
                        Nodes[c, r].Neighbors[MazeNode.North] = Nodes[c, r-1];
                    if (r < rows - 1)
                        Nodes[c, r].Neighbors[MazeNode.South] = Nodes[c, r + 1];
                    if (c > 0)
                        Nodes[c, r].Neighbors[MazeNode.West] = Nodes[c - 1, r];
                    if (c < cols - 1)
                        Nodes[c, r].Neighbors[MazeNode.East] = Nodes[c + 1, r];
                }
            }
            MainWindow.playerCurrentLocation = Nodes[0, 0];
        }

        // Calculate the desired size of the rectangle
        private void CalcRectSize(double StackHeight, double StackWidth, int rows, int cols)
        {
            double HeightSize;
            double WidthSize;
            HeightSize = StackHeight / rows;
            WidthSize = StackWidth / cols;
            //rectSize = Math.Min(HeightSize/1.5, WidthSize/1.5);
            rectSize = Math.Min(HeightSize, WidthSize);
        }

        /// <summary>
        /// Calls all the methods that are needed for the maze generation
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        public void CreateMaze(bool shouldFindSpanningTree)
        {
            double w = screenWidth * 4 / 6;
            double h = screenHeight * 4 / 6;
            CalcRectSize(h, w, rows, cols);
            MakeNodes();
            if (shouldFindSpanningTree)
                Algorithm.FindSpanningTree(Nodes[0, 0]);

            // DrawOnScreen()
        }

        public void DrawOnScreen (StackPanel main)
        {
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
                    Nodes[i, j].GenerateBounds();
                    Nodes[i, j].GenerateBorders();
                    stackPannel.Children.Add(Nodes[i, j].Borders);
                    Nodes[i, j].Borders.Child = Nodes[i, j].Bounds;
                }
                main.Children.Add(stackPannel);
            }
        }
    }
}

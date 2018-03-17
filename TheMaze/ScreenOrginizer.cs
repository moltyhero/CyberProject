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
        MazeNode[,] nodes;

        public ScreenOrginizer(double screenHeight, double screenWidth, int rows, int cols)
        {
            this.screenHeight = screenHeight;
            this.screenWidth = screenWidth;
            this.rows = rows;
            this.cols = cols;
            nodes = new MazeNode[rows, cols];
        }

        // Calculate the desired size of the rectangle
        private double CalcRectSize(double StackHeight, double StackWidth, int rows, int cols)
        {
            double HeightSize;
            double WidthSize;
            HeightSize = StackHeight / rows;
            WidthSize = StackWidth / cols;
            return Math.Min(HeightSize, WidthSize);
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
            double size = CalcRectSize(h, w, rows, cols);

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
                    Rectangle rect = new Rectangle()
                    {
                        Height = size,
                        Width = size,
                        Fill = new SolidColorBrush(System.Windows.Media.Colors.Gray),
                        StrokeThickness = 1,
                        Stroke = Brushes.Violet
                    };
                    stackPannel.Children.Add(rect);
                }

                main.Children.Add(stackPannel);
            }

        }
    }
}

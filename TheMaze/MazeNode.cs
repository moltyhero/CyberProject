using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows;

namespace TheMaze
{
    public class MazeNode
    {
        double boundsSize = ScreenOrginizer.rectSize;
        public const int North = 0;
        public const int South = North + 1;
        public const int East = South + 1;
        public const int West = East + 1;

        // The node's neighbors in order North, South, East, West.
        public MazeNode[] Neighbors = new MazeNode[4];

        // The predecessor in the spanning tree.
        public MazeNode Predecessor = null;

        // The node's bounds.
        public Rectangle Bounds;

        // The node's borders
        public Border Borders;

        public void GenerateBounds()
        {
            this.Bounds = new Rectangle()
            {
                Height = boundsSize,
                Width = boundsSize,
                Fill = new SolidColorBrush(System.Windows.Media.Colors.Gray),
                StrokeThickness = 1,
                Stroke = Brushes.Violet
            };
        }

        public void GenerateBorders()
        {
            this.Borders = new Border()
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 0, 0) // Order - Left, Top, Right, and Bottom
            };
        }

    }
}

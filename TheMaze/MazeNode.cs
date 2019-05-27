using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows;
using ProtoBuf;

namespace TheMaze
{
    public class MazeNode
    {
        double boundsSize = ScreenOrginizer.rectSize;
        public const int North = 0;
        public const int South = North + 1;
        public const int East = South + 1;
        public const int West = East + 1;

        /*public MazeNode (MazeNode[] neighbors, MazeNode predecessor)
        {
            this.Neighbors = neighbors;
            this.Predecessor = predecessor;
        }

        public MazeNode() { }*/

        // The node's neighbors in order North, South, East, West.
        public MazeNode[] Neighbors = new MazeNode[4];

        // The predecessor in the spanning tree.
        public MazeNode Predecessor = null;

        public int predecessoRow, predecessoCol;

        public int col, row;

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
                Fill = new SolidColorBrush(System.Windows.Media.Colors.White),
                /*StrokeThickness = 1,
                Stroke = Brushes.Violet*/
            };
            if (this.Predecessor == this)
                this.Bounds.Fill = new SolidColorBrush(System.Windows.Media.Colors.Black);

            else if (this == ScreenOrginizer.last)
            {
                this.Bounds.Fill = new SolidColorBrush(System.Windows.Media.Colors.Yellow);
            }
                
        }

        public void GenerateBorders()
        {
            int[] sides = new int[4];
            for (int i=0; i<4; i++)
            {
                if (Neighbors[i] != null) // If you either don't have a neighbor there
                {
                    if (Neighbors[i] != Predecessor && Neighbors[i].Predecessor != this) //  If this node is not connected to you by the spanning tree
                    {
                        sides[i] = 1;
                    }
                }
                else sides[i] = 1;
                    
            }
            this.Borders = new Border()
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(sides[3], sides[0], sides[2], sides[1]), // Order - Left, Top, Right, and Bottom
                ClipToBounds = true,
                Height = boundsSize
            };
        }
    }
}

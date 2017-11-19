using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public MainWindow()
        {
            InitializeComponent();
            CreateMaze();
        }

        // Calculate the desired size of the rectangle
        private double calcRectSize(double StackHeight, double StackWidth, int rows, int cols)
        {
            double HeightSize;
            double WidthSize;
            HeightSize = StackHeight / rows;
            WidthSize = StackWidth / cols;
            return Math.Min(HeightSize, WidthSize);
        }

        private void CreateMaze()
        {
            int rows = 20;
            int cols = 22;
            double w = mazeWindow.Width*4/6;
            double h = mazeWindow.Height*4/6;
            double size = calcRectSize(h, w, rows, cols);

            // Add stack pannels to the main stack pannel
            for (int i = 0; i<cols; i++)
            {
                StackPanel stackPannel = new StackPanel()
                {
                    Name = ("col" + i)
                };

                for (int j = 0; j < rows; j++)
                {
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

                mainStackPanel.Children.Add(stackPannel);
            }

        }
    }
    
    
}

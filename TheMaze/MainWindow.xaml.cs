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

        public void CreateMaze()
        {
            
            int rows = 2;
            int cols = 3;
            // Get the maze size
            /*System.Console.WriteLine("Enter row");
            rows = Int32.Parse(Console.ReadLine());
            System.Console.WriteLine("Enter col");
            cols = Int32.Parse(Console.ReadLine());*/



            // Add stack pannels to the stack pannels
            for (int i = 0; i<cols; i++)
            {
                StackPanel stackPannel = new StackPanel();
                stackPannel.Name = ("col" +i);

                for (int j = 0; j < rows; j++)
                {
                    Rectangle rect = new Rectangle();
                    rect.Height = 100;
                    rect.Width = 100;
                    rect.Fill = new SolidColorBrush(System.Windows.Media.Colors.Blue);
                    stackPannel.Children.Add(rect);
                }

                mainStackPanel.Children.Add(stackPannel);
            }

        }
    }
    
    
}

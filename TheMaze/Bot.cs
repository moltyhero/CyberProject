using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheMaze
{
    class Bot
    {
        int currentWayVertically;
        int currentWayHorizontally;
        int currentX;
        int currentY;


        public int CurrentX { get => currentX; set => currentX = value; }
        public int CurrentY { get => currentY; set => currentY = value; }

        public Bot()
        {
            
        }

        public void GenerateLocationAndWay(int boardSizeX, int boardSizeY, int playerX, int playerY)
        {
            Random rand = new Random();
            currentX = rand.Next(0, boardSizeX);
            CurrentY = rand.Next(0, boardSizeY);
            currentWayHorizontally = playerY - boardSizeY;
            currentWayVertically = playerX - boardSizeX;
        }

        public void Move()
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheMaze
{
    public class Player
    {
        public string IP { get; set; }

        public Player (string ip)
        {
            this.IP = ip;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSever.Core
{
    class Player
    {
        public string id;
        public PlayerData data;
        public Player()
        {
            this.data = new PlayerData();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSever.Core
{
    [Serializable]
    class PlayerData
    {
        public int source = 0;
        public PlayerData()
        {
            this.source = 0;
        }
    }
}

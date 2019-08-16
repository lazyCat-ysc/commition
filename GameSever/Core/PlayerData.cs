using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSever.Core
{
    [Serializable]
    class PlayersubCmd
    {
        public int source = 0;
        public PlayersubCmd()
        {
            this.source = 0;
        }
    }
}

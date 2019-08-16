using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSever.Core
{
    [Serializable]
    class ProtocolBase
    {
        public virtual ProtocolBase Decode(byte[] readBuffer, int start , int length)
        {
            return new ProtocolBase();
        }
        public virtual byte[] Encode()
        {
            return new byte[] { };
        }
        public virtual string GetName()
        {
            return "";
        }
        public virtual string GetDesc()
        {
            return "";
        }
    }
}

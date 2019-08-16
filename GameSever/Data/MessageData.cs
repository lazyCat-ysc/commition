using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameSever.Core;

namespace GameSever.Data
{
    [Serializable]
    class MessageData
    {
        [Serializable]
        public struct Data
        {
            public int mainCmdId;
            public int subCmdId;
            public ProtocolBytes data;
        }
        public Data data;
        public MessageData()
        {
            data = new Data
            {
                data = new ProtocolBytes(),
                mainCmdId = 0,
                subCmdId = 0
            };
        }

        public int SetMainCmdId
        {
            set
            {
                data.mainCmdId = value;
            }
            get
            {
                return data.mainCmdId;
            }
        }

        public int SetSubCmdId
        {
            set
            {
                data.subCmdId = value;
            }
            get
            {
                return data.subCmdId;
            }
        }

        public ProtocolBytes SetsubCmd
        {
            set
            {
                data.data = value;
            }
            get
            {
                return data.data;
            }
        }
    }
}

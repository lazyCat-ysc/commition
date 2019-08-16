using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameSever.Core;
using GameSever.Data;

namespace GameSever.Logic
{
    class HandleConnect
    {
        public void HandleUpdateHeartBeat(Connection con,MessageData messageData)
        {
            con.lastTicketTime = Sys.GetTimeStamp();
           // Console.WriteLine("更新心跳");
        }
        public void HandleTalk(Connection con, MessageData messageData)
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString(con.GetAddress());
            protocol.AddString(Sys.GetNowTime());
            protocol.AddString(messageData.data.data.GetString());
            messageData.data.data = protocol;
            SeverNet.instance.Broadcast(con, messageData);
        }
    }
}

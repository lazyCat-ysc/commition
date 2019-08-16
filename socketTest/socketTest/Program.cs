using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameSever.Core;
using GameSever.Data;

namespace socketTest
{
    class Program
    {
        static void Main(string[] args)
        {
            SeverNet sever = new SeverNet();
            sever.Start("127.0.0.1", 9999);
            bool isSend = false;
            while (true)
            {
                //Console.WriteLine(Sys.GetNowTime());
                if (isSend)
                {
                    isSend = false;
                }
                else
                {
                    Console.WriteLine("");
                    Console.Write("发送消息:");
                    string str = Console.ReadLine();
                    MessageData messageData = new MessageData();
                    messageData.data.mainCmdId = 0;
                    messageData.data.subCmdId = 1;
                    ProtocolBytes protocolBytes = new ProtocolBytes();
                    protocolBytes.AddString(str);
                    messageData.data.data = protocolBytes;
                    sever.Send(messageData);
                    str = "";
                    isSend = true;
                }
                
            }
        }
    }
}

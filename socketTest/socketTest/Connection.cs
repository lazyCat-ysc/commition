using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace GameSever.Core
{
    class Connection
    {
        public Socket socket;
        public byte[] readBuffer;
        public bool isUsed = false;
        public const int BUFFER_SIZE = 1024;
        public int bufferCount = 0;
        #region 粘包分包
        public byte[] lenBytes = new byte[sizeof(Int32)];
        public Int32 msgLength = 0;
        #endregion
        #region 心跳协议
        public long lastTicketTime = long.MinValue;
        #endregion
        public Player player;

        public Connection()
        {
            this.readBuffer = new byte[BUFFER_SIZE];
        }
        public void Init(Socket socket)
        {
            this.socket = socket;
            isUsed = true;
            bufferCount = 0;
            lastTicketTime = Sys.GetTimeStamp();
        }
        public int Buffremain()
        {
            return BUFFER_SIZE - bufferCount;
        }
        public string GetAddress()
        {
            if (!isUsed)
                return "无法获取地址";
            return socket.RemoteEndPoint.ToString();
        }
        public void Close()
        {
            if (!isUsed)
                return;
            if (player != null)
            {
                return;
            }
            Console.WriteLine("断开连接:" + GetAddress());
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            isUsed = false;

        }
    }   
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using GameSever.Serializable;
using GameSever.Data;

namespace GameSever.Core
{
    class SeverNet
    {
        public Socket listenfd;
        public Connection[] conns;
        public int maxConn = 50;
        public static SeverNet instance;
        Timer timer = new Timer(10000);
        public long heartBeatTime = 180;
        public ProtocolBase protocol;
        const int BUFFER_SIZE = 1024;
        public Socket socket;
        public string recvStr;
        Int32 msgLength = 0;
        int buffCount = 0;
        byte[] lenBytes = new byte[sizeof(UInt32)];
        public byte[] readBuff = new byte[BUFFER_SIZE];
        public SeverNet()
        {
            instance = this;
        }

        private void ReceiveCb(IAsyncResult ar)
        {
            try
            {
                //count是接收数据的大小
                int count = socket.EndReceive(ar);
                //数据处理
                buffCount += count;
                ProcessData();
                //继续接收	
                socket.BeginReceive(readBuff, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCb, socket);
            }
            catch (Exception e)
            {
                Console.WriteLine("链接已断开");
                socket.Close();
            }
        }

        public void Start(string host, int port)
        {
            //定时器
            timer.Elapsed += new System.Timers.ElapsedEventHandler(HandleMainTimer);
            timer.AutoReset = false;
            timer.Enabled = true;

            //Socket
            socket = new Socket(AddressFamily.InterNetwork,
                             SocketType.Stream, ProtocolType.Tcp);
            //Connect
            
            socket.Connect(host, port);
            Console.WriteLine("客户端地址 " + socket.LocalEndPoint.ToString());
            //Recv
            socket.BeginReceive(readBuff, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCb, null);
            
        }
        
        public void Close()
        {
            for(int i = 0;i<conns.Length;i++)
            {
                Connection con = conns[i];
                if (con == null)
                    continue;
                if (!con.isUsed)
                    continue;
                lock(con)
                {
                    con.Close();
                }
            }
        }
        private void ProcessData()
        {
            if (buffCount < sizeof(Int32))
                return;
            Array.Copy(readBuff, lenBytes, sizeof(Int32));
            msgLength = BitConverter.ToInt32(lenBytes, 0);
            if (buffCount < msgLength + sizeof(Int32))
                return;
            Serial ser = new Serial();
            MessageData message = new MessageData();
            message = (MessageData)ser.Decode(readBuff, sizeof(Int32), msgLength);
            HandleMainMsg(message);
            //ProtocolBytes proto = message.data.data;
            //Console.WriteLine(message.data.mainCmdId);
            //Console.WriteLine(message.data.subCmdId);
            //Console.WriteLine(proto.GetString());
            //Console.WriteLine(proto.GetString());
            //Console.WriteLine(proto.GetInt());
            int count = buffCount - msgLength - sizeof(Int32);
            Array.Copy(readBuff, msgLength, readBuff, 0, count);
            buffCount = count;
            if (buffCount > 0)
            {
                ProcessData();
            }
        }

        private void HandleMainMsg(MessageData messageData)
        {
            if (messageData.data.mainCmdId == 0)
            {
                HandleSubMsg(messageData);
            }
        }

        private void HandleSubMsg(MessageData messageData)
        {

            ShowMessage(messageData.data.data, messageData.data.subCmdId);
        }


        private void ShowMessage(ProtocolBytes data, int cmd)
        {
            Console.WriteLine("");
            Console.WriteLine("");
            switch(cmd)
            {
                case 1:
                    {
                        Console.WriteLine("收到发消息Ip:[" + data.GetString() + "]" + "日期:" + data.GetString());
                        Console.WriteLine("消息:" + data.GetString());
                        break;
                    }
                case 2:
                case 3:
                    {
                        Console.WriteLine(data.GetString());
                        break;
                    }
            }
            
            Console.WriteLine("");
            Console.Write("发送消息:");
        }

        private void HandleMsg(Connection con, ProtocolBase protocolBase)
        {
            //string name = protocolBase.GetName();
            //Console.WriteLine("[收到协议]:" +name);
            //if(name == "HeartBeat")
            //{
            //    Console.WriteLine("[更新心跳时间]:" + con.GetAddress());
            //    con.lastTicketTime = Sys.GetTimeStamp();
            //}
        }
        //public void Broadcast(ProtocolBase protocol)
        //{
        //    for(int i =0; i < conns.Length;i++)
        //    {
        //        if (conns[i] == null) continue;
        //        if (!conns[i].isUsed) continue;
        //        Send(conns[i],protocol);
        //    }
        //}
        public bool Send(MessageData messageData)
        {
            
            Serial ser = new Serial();
            byte[] buff = ser.Encode(messageData);
            byte[] buffLen = BitConverter.GetBytes(buff.Length);
            byte[] sendBuff = buffLen.Concat(buff).ToArray();
            uint len = BitConverter.ToUInt32(sendBuff, 0);
            try
            {
                socket.BeginSend(sendBuff, 0, sendBuff.Length, SocketFlags.None, null, null);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[SeverNet] Send:" + e.Message);
                socket.Close();
                return false;
            }
        }

        public bool Send(Connection con, string str)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
            byte[] length = BitConverter.GetBytes(bytes.Length);
            byte[] sendBuff = length.Concat(bytes).ToArray();
            try
            {
                con.socket.BeginSend(sendBuff,0,sendBuff.Length,SocketFlags.None,null,null);
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("[SeverNet] Send:" + con.GetAddress() + e.Message);
                return false;
            }
        }
        public void HandleMainTimer(object sender,ElapsedEventArgs e)
        {
            SendHeartBeat();
            timer.Start();
        }
        public void SendHeartBeat()
        {
            MessageData messageData = new MessageData();
            messageData.data.mainCmdId = 0;
            messageData.data.subCmdId = 0;
            ProtocolBytes protocolBytes = new ProtocolBytes();
            messageData.data.data = protocolBytes;
            Send(messageData);
            ////Console.WriteLine("[主定时器执行]");
            //long timeNow = Sys.GetTimeStamp();
            //for(int i = 0;i < conns.Length; i++)
            //{
            //    Connection con = conns[i];
            //    if (con == null) continue;
            //    if (!con.isUsed) continue;
            //    //Console.WriteLine(con.lastTicketTime);
            //    //Console.WriteLine(timeNow - heartBeatTime);
            //    if (con.lastTicketTime < timeNow - heartBeatTime)
            //    {
            //        Console.WriteLine("心跳引起断开");
            //        lock(con)
            //        {
            //           con.Close();
            //        }

            //    }
            //}
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace GameSever.Core
{
    class SeverNet
    {
        public Socket listenfd;
        public Connection[] conns;
        public int maxConn = 50;
        public static SeverNet instance;
        Timer timer = new Timer(1000);
        public long heartBeatTime = 180;
        public ProtocolBase protocol;
        public SeverNet()
        {
            instance = this;
        }
        public int NewIndex()
        {
            if (conns == null)
                return -1;
            for (int i = 0; i < conns.Length; i++)
            {
                if (conns[i] == null)
                {
                    conns[i] = new Connection();
                    return i;
                }
                else if (conns[i].isUsed == false)
                {
                    return i;
                }
            }
            return -1;
        }

        private void ReceiveCb(IAsyncResult ar)
        {
            Connection conn = (Connection)ar.AsyncState;
            lock (conn)
            {
                try
                {
                    int count = conn.socket.EndReceive(ar);
                    //关闭信号
                    if (count <= 0)
                    {
                        Console.WriteLine("收到 [" + conn.GetAddress() + "] 断开链接");
                        conn.Close();
                        return;
                    }
                    conn.bufferCount += count;
                    ProcessData(conn);
                    //继续接收	
                    conn.socket.BeginReceive(conn.readBuffer,
                                             conn.bufferCount, conn.Buffremain(),
                                             SocketFlags.None, ReceiveCb, conn);
                    //Console.WriteLine("收到："+ conn.readBuffer.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("收到 [" + conn.GetAddress() + "] 断开链接");
                    conn.Close();
                }
            }
        }

        public void Start(string host, int port)
        {
            //定时器
            timer.Elapsed += new System.Timers.ElapsedEventHandler(HandleMainTimer);
            timer.AutoReset = false;
            timer.Enabled = true;
            //链接池
            conns = new Connection[maxConn];
            for (int i = 0; i < maxConn; i++)
            {
                conns[i] = new Connection();
            }
            //Socket
            listenfd = new Socket(AddressFamily.InterNetwork,
                                  SocketType.Stream, ProtocolType.Tcp);
            //Bind
            IPAddress ipAdr = IPAddress.Parse(host);
            IPEndPoint ipEp = new IPEndPoint(ipAdr, port);
            listenfd.Bind(ipEp);
            //Listen
            listenfd.Listen(maxConn);
            //Accept
            listenfd.BeginAccept(AcceptCb, null);
            Console.WriteLine("[服务器]启动成功");
        }
        private void AcceptCb(IAsyncResult ar)
        {
            try
            {
                Socket socket = listenfd.EndAccept(ar);
                int index = NewIndex();

                if (index < 0)
                {
                    socket.Close();
                    Console.Write("[警告]链接已满");
                }
                else
                {
                    Connection conn = conns[index];
                    conn.Init(socket);
                    string adr = conn.GetAddress();
                    Console.WriteLine("客户端连接 [" + adr + "] conn池ID：" + index);
                    conn.socket.BeginReceive(conn.readBuffer,
                                             conn.bufferCount, conn.Buffremain(),
                                             SocketFlags.None, ReceiveCb, conn);
                }
                listenfd.BeginAccept(AcceptCb, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("AcceptCb失败:" + e.Message);
            }
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
        private void ProcessData(Connection con)
        {
            if (con.bufferCount < sizeof(Int32))
                return;
            Array.Copy(con.readBuffer,con.lenBytes,sizeof(Int32));
            con.msgLength = BitConverter.ToInt32(con.lenBytes,0);
            if (con.bufferCount < con.msgLength + sizeof(Int32))
                return;
            ProtocolBase proto = protocol.Decode(con.readBuffer,sizeof(Int32),con.msgLength);
            HandleMsg(con,proto);
            //string str = System.Text.Encoding.UTF8.GetString(con.readBuffer,sizeof(Int32),con.msgLength);
            //Console.WriteLine("收到[" + con.GetAddress() + "]:" + str);
            //if (str == "HeartBeat")
            //    con.lastTicketTime = Sys.GetTimeStamp();
            int count = con.bufferCount - con.msgLength - sizeof(Int32);
            Array.Copy(con.readBuffer,con.msgLength + sizeof(Int32),con.readBuffer,0,count);
            con.bufferCount = count;
            if (con.bufferCount > 0)
                ProcessData(con);

        }
        private void HandleMsg(Connection con, ProtocolBase protocolBase)
        {
            string name = protocolBase.GetName();
            Console.WriteLine("[收到协议]:" +name);
            if(name == "HeartBeat")
            {
                Console.WriteLine("[更新心跳时间]:" + con.GetAddress());
                con.lastTicketTime = Sys.GetTimeStamp();
            }
        }
        public void Broadcast(ProtocolBase protocol)
        {
            for(int i =0; i < conns.Length;i++)
            {
                if (conns[i] == null) continue;
                if (!conns[i].isUsed) continue;
                Send(conns[i],protocol);
            }
        }
        public bool Send(Connection con, ProtocolBase protocol)
        {
            byte[] bytes = protocol.Encode();
            byte[] length = BitConverter.GetBytes(bytes.Length);
            byte[] sendBuff = length.Concat(bytes).ToArray();
            try
            {
                con.socket.BeginSend(sendBuff, 0, sendBuff.Length, SocketFlags.None, null, null);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[SeverNet] Send:" + con.GetAddress() + e.Message);
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
            HeartBeat();
            timer.Start();
        }
        public void HeartBeat()
        {
            //Console.WriteLine("[主定时器执行]");
            long timeNow = Sys.GetTimeStamp();
            for(int i = 0;i < conns.Length; i++)
            {
                Connection con = conns[i];
                if (con == null) continue;
                if (!con.isUsed) continue;
                //Console.WriteLine(con.lastTicketTime);
                //Console.WriteLine(timeNow - heartBeatTime);
                if (con.lastTicketTime < timeNow - heartBeatTime)
                {
                    Console.WriteLine("心跳引起断开");
                    lock(con)
                    {
                       con.Close();
                    }
                    
                }
            }
        }
    }
}

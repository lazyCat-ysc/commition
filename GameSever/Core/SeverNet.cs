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
using GameSever.Logic;
using System.Reflection;

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
        private HandleConnect handleConnect;
        private SeverData severData;
        public SeverNet()
        {
            handleConnect = new HandleConnect();
            instance = this;
        }
        ~SeverNet()
        {
            Close();
        }

        private int GetOnlineCount()
        {
            int count = 0;
            for (int i = 0;i<conns.Length;i++)
            {
                if (conns[i] == null) continue;
                if (!conns[i].isUsed) continue;
                count += 1;
            }
            return count;
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
                    MessageData messageData = new MessageData();
                    string msg = "收到 [" + conn.GetAddress() + "] 退出聊天房间";
                    AddMessage(conn,ref messageData, 0,2, msg);
                    Broadcast(conn, messageData);
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
                    MessageData messageData = new MessageData();
                    string msg = " [" + conn.GetAddress() + "] 加入聊天房间,当前房间人数[" + GetOnlineCount() + "]";
                    AddMessage(conn, ref messageData, 0, 3, msg);
                    Broadcast(conn, messageData,true);
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
            
            lock(con)
            {
                Serial ser = new Serial();
                MessageData message = new MessageData();
                message = (MessageData)ser.Decode(con.readBuffer, sizeof(Int32), con.msgLength);
                HandleMainMsg(con, message);
            }
            int count = con.bufferCount - con.msgLength - sizeof(Int32);
            Array.Copy(con.readBuffer, con.msgLength + sizeof(Int32), con.readBuffer, 0, count);
            con.bufferCount = count;
            if (con.bufferCount > 0)
                ProcessData(con);
        }

        private void HandleMainMsg(Connection con, MessageData messageData)
        {
            MethodInfo mm = this.GetType().GetMethod(SeverData.instance.GetMainCmd(messageData.data.mainCmdId));
            Object[] obj = new object[] { con, messageData };
            mm.Invoke(this, obj);
            //if (messagesubCmd.subCmd.mainCmdId == 0)
            //{
            //    HandleSubMsg(con, messagesubCmd);
            //}
        }

        public void HandleSysMsg(Connection con, MessageData messageData)
        {
            MethodInfo mm = handleConnect.GetType().GetMethod(SeverData.instance.GetSubCmd(messageData.data.subCmdId));
            Object[] obj = new object[] { con, messageData };
            mm.Invoke(handleConnect,obj);
            //if (messagesubCmd.subCmd.subCmdId == 0)
            //{
            //    //Console.WriteLine("[更新心跳时间]:" + con.GetAddress());
            //    con.lastTicketTime = Sys.GetTimeStamp();
            //}
            //else if(messagesubCmd.subCmd.subCmdId == 1)
            //{
            //    FixedMessage(con,ref messagesubCmd);
            //    Broadcast(con, messagesubCmd);
            //}
        }

        private void AddMessage(Connection con, ref MessageData message,int mainCmd, int subCmd, string msg)
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString(msg);
            message.data.mainCmdId = mainCmd;
            message.data.subCmdId = subCmd;
            message.data.data = protocol;
        }

        private void FixedMessage(Connection con,ref MessageData message)
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString(con.GetAddress());
            protocol.AddString(Sys.GetNowTime());
            protocol.AddString(message.data.data.GetString());
            message.data.data = protocol;
        }

        public void Broadcast(Connection con, MessageData message,bool isTalkSelf = false)
        {
            for (int i = 0; i < conns.Length; i++)
            {
                if (conns[i] == null) continue;
                if (!conns[i].isUsed) continue;
                if (conns[i] == con && !isTalkSelf) continue;
                lock(conns[i])
                {
                    Send(conns[i], message);
                }
            }
        }

        private void HandleMsg(Connection con, ProtocolBase protocolBase)
        {
            string name = protocolBase.GetName();
            ProtocolBytes pro = (ProtocolBytes)protocolBase;
            int num = pro.GetInt(0);
            Console.WriteLine("[收到协议]:" +name + num);
            if(name == "HeartBeat")
            {
               // Console.WriteLine("[更新心跳时间]:" + con.GetAddress());
                con.lastTicketTime = Sys.GetTimeStamp();
            }
        }

        private void SetMessagesubCmd()
        {

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

        public bool Send(Connection con, MessageData messageData)
        {
            Serial ser = new Serial();
            byte[] buff = ser.Encode(messageData);
            byte[] buffLen = BitConverter.GetBytes(buff.Length);
            byte[] sendBuff = buffLen.Concat(buff).ToArray();
            uint len = BitConverter.ToUInt32(sendBuff, 0);
            try
            {
                con.socket.BeginSend(sendBuff, 0, sendBuff.Length, SocketFlags.None, SendCallBack, con);
                return true;
            }
            catch (Exception e)
            {
                MessageData message = new MessageData();
                string msg = "收到 [" + con.GetAddress() + "] 退出聊天房间";
                AddMessage(con, ref message, 0, 2, msg);
                Broadcast(con, message);
                Console.WriteLine("[SeverNet] Send:" + e.Message);
                con.Close();
                return false;
            }
        }

        private void SendCallBack(IAsyncResult ar)
        {
            Connection conn = (Connection)ar.AsyncState;
            lock (conn)
            {
               // Console.WriteLine(conn.socket.EndSend(ar));
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
                Console.WriteLine("[SeverNet] Send:" + e.Message);
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
                Console.WriteLine("[SeverNet] Send:"+ e.Message);
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
                        MessageData messageData = new MessageData();
                        string msg = "收到 [" + con.GetAddress() + "] 退出聊天房间";
                        AddMessage(con, ref messageData, 0, 2, msg);
                        Broadcast(con, messageData);
                        con.Close();
                    }
                    
                }
            }
        }
    }
}

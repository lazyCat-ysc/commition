using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MySql.Data;
using GameSever.Core;
using GameSever.Data;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using GameSever.Serializable;
using GameSever.Operation;

namespace GameSever
{
    class Program
    {
        static void Main(string[] args)
        {
            //XmlOperation xml = new XmlOperation();
            //xml.XmlRead();
            SeverData cmd = new SeverData();
            cmd.ReadConfig();
            SeverNet sever = new SeverNet();
            sever.Start("127.0.0.1",9999);
            sever.protocol = new ProtocolBytes();
            while (true) ;
        }
    }
}

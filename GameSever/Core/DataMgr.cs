using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MySql.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace GameSever.Core
{
    class subCmdMgr
    {
        private MySqlConnection sqlCon;
        public static subCmdMgr instance;

        public subCmdMgr()
        {
            instance = this;
            Connect();
        }
        #region 连接mysql
        private void Connect()
        {
            string sqlCom = "subCmdbase=game;subCmdSource=127.0.0.1;User Id=root;Password=64450252;Port=3306";
            sqlCon = new MySqlConnection(sqlCom);
            try
            {
                Console.WriteLine("[subCmdMgr] Connect: connect Succeed");
                sqlCon.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine("[subCmdMgr] Connect:" + e.Message);
                return;
            }
        }
        #endregion
        #region 是否可以注册
        private bool CanRegister(string id)
        {
            string cmdStr = string.Format("select * from user where id = '{0}';", id);
            MySqlCommand sqlCom = new MySqlCommand(cmdStr, sqlCon);
            try
            {
                MySqlDataReader subCmdReader = sqlCom.ExecuteReader();
                bool hasRegister = subCmdReader.HasRows;
                subCmdReader.Close();
                return !hasRegister;
            }
            catch(Exception e)
            {
                Console.WriteLine("[subCmdMgr] CanRegister:" + e.Message);
                return false;
            }
        }
        #endregion
        #region 注册用户
        public bool Register(string id, string pw)
        {
            if (!CanRegister(id))
            {
                Console.WriteLine("[subCmdMgr] Register : id has alread register");
                return false;
            }
            string cmdStr = string.Format("insert into user set id='{0}',pw='{1}';", id, pw);
            MySqlCommand sqlCom = new MySqlCommand(cmdStr, sqlCon);
            try
            {
                sqlCom.ExecuteNonQuery();
                return true;
            }
            catch(Exception e)
            {
                
                Console.WriteLine("[subCmdMgr] Register:" + e.Message);
                return false;
            }
        }
        #endregion
        #region 创建角色
        public bool CreatePlayer(string id)
        {
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            PlayersubCmd playersubCmd = new PlayersubCmd();
            try
            {
                formatter.Serialize(stream,playersubCmd);
            }
            catch(SerializationException e)
            {
                Console.WriteLine("[subCmdMgr] CreatePlayer 序列化:" + e.Message);
                return false;
            }
            byte[] byteArray = stream.ToArray();
            string cmdStr = string.Format("insert into player set id='{0}', subCmd=@subCmd;", id);
            MySqlCommand sqlCom = new MySqlCommand(cmdStr,sqlCon);
            sqlCom.Parameters.Add("@subCmd", MySqlDbType.Blob);
            sqlCom.Parameters[0].Value = byteArray;
            try
            {
                sqlCom.ExecuteNonQuery();
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("[subCmdMgr] CreatePlayer:" + e.Message);
                return false;
            }
        }
        #endregion
        #region 登入
        public bool CheckPassWord(string id , string pw)
        {
            string cmdStr = string.Format("select * from user where id='{0}' and pw='{1}';",id,pw);
            MySqlCommand sqlCom = new MySqlCommand(cmdStr,sqlCon);
            try
            {
                MySqlDataReader subCmdReader = sqlCom.ExecuteReader();
                bool loginSucess = subCmdReader.HasRows;
                subCmdReader.Close();
                return loginSucess;
            }
            catch(Exception e)
            {
                Console.WriteLine("[subCmdMgr] CheckPassword:" + e.Message);
                return false;
            }
        }
        #endregion
        #region 获取玩家数据
        public PlayersubCmd GetPlayersubCmd(string id)
        {
            PlayersubCmd playersubCmd = new PlayersubCmd();
            string cmdStr = string.Format("select * from player where id='{0}';",id);
            MySqlCommand sqlCom = new MySqlCommand(cmdStr, sqlCon);
            byte[] buffer = new byte[1];
            try
            {
                MySqlDataReader subCmdReader = sqlCom.ExecuteReader();
                if (!subCmdReader.HasRows)
                {
                    subCmdReader.Close();
                    return playersubCmd;
                }
                subCmdReader.Read();
                long len = subCmdReader.GetBytes(1, 0, null, 0, 0);
                buffer = new byte[len];
                subCmdReader.GetBytes(1,0,buffer,0,(int)len);
                subCmdReader.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine("[subCmdMgr] GetPlayersubCmd:" + e.Message);
                return playersubCmd;
            }
            MemoryStream stream = new MemoryStream(buffer);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                playersubCmd = (PlayersubCmd)formatter.Deserialize(stream);
                return playersubCmd;
            }
            catch(SerializationException e)
            {
                Console.WriteLine("[subCmdMgr] GetPlayersubCmd 反序列化:" + e.Message);
                return playersubCmd;
            }
        }
        #endregion
        #region 保存数据
        public bool SavePlayer(Player player)
        {
            string id = player.id;
            PlayersubCmd playersubCmd = player.subCmd;
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            try
            {
                formatter.Serialize(stream,playersubCmd);
            }
            catch(SerializationException e)
            {
                Console.WriteLine("[subCmdMgr] SavePlayer 序列化:" + e.Message);
                return false;
            }
            byte[] byteArray = stream.ToArray();
            string cmdStr = string.Format("update player set subCmd=@subCmd where id='{0}';",id);
            MySqlCommand sqlCom = new MySqlCommand(cmdStr, sqlCon);
            sqlCom.Parameters.Add("@subCmd", MySqlDbType.Blob);
            sqlCom.Parameters[0].Value = byteArray;
            try
            {
                sqlCom.ExecuteNonQuery();
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("[subCmdMgr] SavePlayer:" + e.Message);
                return false;
            }
        }
        #endregion
    }
}

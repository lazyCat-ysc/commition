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
    class DataMgr
    {
        private MySqlConnection sqlCon;
        public static DataMgr instance;

        public DataMgr()
        {
            instance = this;
            Connect();
        }
        #region 连接mysql
        private void Connect()
        {
            string sqlCom = "Database=game;DataSource=127.0.0.1;User Id=root;Password=64450252;Port=3306";
            sqlCon = new MySqlConnection(sqlCom);
            try
            {
                Console.WriteLine("[DataMgr] Connect: connect Succeed");
                sqlCon.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr] Connect:" + e.Message);
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
                MySqlDataReader dataReader = sqlCom.ExecuteReader();
                bool hasRegister = dataReader.HasRows;
                dataReader.Close();
                return !hasRegister;
            }
            catch(Exception e)
            {
                Console.WriteLine("[DataMgr] CanRegister:" + e.Message);
                return false;
            }
        }
        #endregion
        #region 注册用户
        public bool Register(string id, string pw)
        {
            if (!CanRegister(id))
            {
                Console.WriteLine("[DataMgr] Register : id has alread register");
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
                
                Console.WriteLine("[DataMgr] Register:" + e.Message);
                return false;
            }
        }
        #endregion
        #region 创建角色
        public bool CreatePlayer(string id)
        {
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            PlayerData playerData = new PlayerData();
            try
            {
                formatter.Serialize(stream,playerData);
            }
            catch(SerializationException e)
            {
                Console.WriteLine("[DataMgr] CreatePlayer 序列化:" + e.Message);
                return false;
            }
            byte[] byteArray = stream.ToArray();
            string cmdStr = string.Format("insert into player set id='{0}', data=@data;", id);
            MySqlCommand sqlCom = new MySqlCommand(cmdStr,sqlCon);
            sqlCom.Parameters.Add("@data", MySqlDbType.Blob);
            sqlCom.Parameters[0].Value = byteArray;
            try
            {
                sqlCom.ExecuteNonQuery();
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("[DataMgr] CreatePlayer:" + e.Message);
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
                MySqlDataReader dataReader = sqlCom.ExecuteReader();
                bool loginSucess = dataReader.HasRows;
                dataReader.Close();
                return loginSucess;
            }
            catch(Exception e)
            {
                Console.WriteLine("[DataMgr] CheckPassword:" + e.Message);
                return false;
            }
        }
        #endregion
        #region 获取玩家数据
        public PlayerData GetPlayerData(string id)
        {
            PlayerData playerData = new PlayerData();
            string cmdStr = string.Format("select * from player where id='{0}';",id);
            MySqlCommand sqlCom = new MySqlCommand(cmdStr, sqlCon);
            byte[] buffer = new byte[1];
            try
            {
                MySqlDataReader dataReader = sqlCom.ExecuteReader();
                if (!dataReader.HasRows)
                {
                    dataReader.Close();
                    return playerData;
                }
                dataReader.Read();
                long len = dataReader.GetBytes(1, 0, null, 0, 0);
                buffer = new byte[len];
                dataReader.GetBytes(1,0,buffer,0,(int)len);
                dataReader.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine("[DataMgr] GetPlayerData:" + e.Message);
                return playerData;
            }
            MemoryStream stream = new MemoryStream(buffer);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                playerData = (PlayerData)formatter.Deserialize(stream);
                return playerData;
            }
            catch(SerializationException e)
            {
                Console.WriteLine("[DataMgr] GetPlayerData 反序列化:" + e.Message);
                return playerData;
            }
        }
        #endregion
        #region 保存数据
        public bool SavePlayer(Player player)
        {
            string id = player.id;
            PlayerData playerData = player.data;
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            try
            {
                formatter.Serialize(stream,playerData);
            }
            catch(SerializationException e)
            {
                Console.WriteLine("[DataMgr] SavePlayer 序列化:" + e.Message);
                return false;
            }
            byte[] byteArray = stream.ToArray();
            string cmdStr = string.Format("update player set data=@data where id='{0}';",id);
            MySqlCommand sqlCom = new MySqlCommand(cmdStr, sqlCon);
            sqlCom.Parameters.Add("@data", MySqlDbType.Blob);
            sqlCom.Parameters[0].Value = byteArray;
            try
            {
                sqlCom.ExecuteNonQuery();
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("[DataMgr] SavePlayer:" + e.Message);
                return false;
            }
        }
        #endregion
    }
}

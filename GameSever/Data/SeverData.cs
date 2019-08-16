using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GameSever.Data
{
    class SeverData
    {
        public static SeverData instance;
        public Dictionary<Int32,string> subCmd;
        public Dictionary<Int32, string> mainCmd;
        public SeverData()
        {
            mainCmd = new Dictionary<int, string>();
            subCmd = new Dictionary<int, string>();
            //ReadConfig();
            instance = this;
        }

        public void AddMainCmd(Int32 num, string msg)
        {
            if (!mainCmd.ContainsKey(num))
                mainCmd.Add(num, msg);
        }

        public bool FindMainCmd(Int32 num)
        {
            if (mainCmd.Count > 0 && mainCmd.ContainsKey(num))
                return true;
            return false;
        }

        public string GetMainCmd(Int32 num)
        {
            if (FindMainCmd(num))
            {
                return mainCmd.ElementAt(num).Value;
            }
            return "";
        }

        public void AddSubCmd(Int32 num, string msg)
        {
            if (!subCmd.ContainsKey(num))
                subCmd.Add(num, msg);
        }

        public bool FindSubCmd(Int32 num)
        {
            if (subCmd.Count > 0 && subCmd.ContainsKey(num))
                return true;
            return false;
        }

        public string GetSubCmd(Int32 num)
        {
            if (FindSubCmd(num))
            {
                return subCmd.ElementAt(num).Value;
            }
            return "";
        }

        public void ReadConfig()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("./Config/SeverCmd.xml");
            XmlNodeList list = doc.SelectNodes("/body/mainCmd/cmd");
            foreach (XmlNode item in list)
            {
                AddMainCmd(Convert.ToInt32(item.Attributes["key"].Value), item.Attributes["name"].Value);
            }
            list = doc.SelectNodes("/body/subCmd/cmd");
            foreach (XmlNode item in list)
            {
                AddSubCmd(Convert.ToInt32(item.Attributes["key"].Value), item.Attributes["name"].Value);
            }
        }
    }
}

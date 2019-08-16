using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GameSever.Operation
{
    class XmlOperation
    {
        public void XmlRead()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("./Config/SubComId.xml");
            string t = doc.Value;
            XmlNodeList list = doc.SelectNodes("/body/item");



            foreach (XmlNode item in list)
            {
                int i = 0;
                string q = item.Attributes["name"].Value;
                string id = item["new"].InnerText;
                int yy = 0;
                //CustomerInfo cust = new CustomerInfo();
                //cust.Version = item.Attributes["Version"].Value;
                //cust.AppId = item.Attributes["AppId"].Value;
                //cust.CustomerID = item["CustomerID"].InnerText;
                //cust.CompanyName = item["CompanyName"].InnerText;
                //cust.ContactName = item["ContactName"].InnerText;
                //cust.ContactTitle = item["ContactTitle"].InnerText;
                //cust.Address = item["Address"].InnerText;
                //cust.City = item["City"].InnerText;
                //cust.PostalCode = item["PostalCode"].InnerText;
                //cust.Country = item["Country"].InnerText;
                //cust.Phone = item["Phone"].InnerText;
                //cust.Fax = item["Fax"].InnerText;
                //lists.Add(cust);
            }
        }
    }
}

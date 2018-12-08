using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Web;

namespace Angels.Common
{
    public class ErrorLog
    {
        public ErrorLog(string Function, string ErrorInfo, string Description)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(HttpContext.Current.Server.MapPath("/Log/ErrorLog.xml"));

            XmlNode root = xmlDoc.SelectSingleNode("DataItem");

            XmlElement xe1 = xmlDoc.CreateElement("ErrorLog");

            XmlElement xe1sub1 = xmlDoc.CreateElement("ErrorDate");
            xe1sub1.InnerText = DateTime.Now.ToString();
            xe1.AppendChild(xe1sub1);

            XmlElement xe1sub2 = xmlDoc.CreateElement("ErrorInfo");
            xe1sub2.InnerText = ErrorInfo;
            xe1.AppendChild(xe1sub2);            

            XmlElement xe1sub3 = xmlDoc.CreateElement("ErrorFunction");
            xe1sub3.InnerText = Function;
            xe1.AppendChild(xe1sub3);

            XmlElement xe1sub4 = xmlDoc.CreateElement("Description");
            xe1sub4.InnerText = ErrorInfo;
            xe1.AppendChild(xe1sub4);

            root.AppendChild(xe1);//添加到<Data>节点中

            xmlDoc.Save(HttpContext.Current.Server.MapPath("/Log/ErrorLog.xml"));
        }
    }
}

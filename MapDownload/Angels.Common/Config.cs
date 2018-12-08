using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Xml;
using System.Web;

namespace Angels.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class Config
    {
        /// <summary>
        /// 配置文件相对路径。
        /// </summary>
        private const string CONGIG_PATH = "XmlConfig\\AppSetting.config";

        /// <summary>
        /// 根据Key取Value值。
        /// </summary>
        /// <param name="key">键</param>
        public static string GetValue(string key)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(System.AppDomain.CurrentDomain.BaseDirectory + CONGIG_PATH);
            return doc["appSettings"][key].InnerText;
            //return ConfigurationManager.AppSettings[key].ToString().Trim();
        }
        /// <summary>
        /// 根据Key修改Value。
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public static void SetValue(string key, string value)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(System.AppDomain.CurrentDomain.BaseDirectory + CONGIG_PATH);
            XmlNode xNode;
            xNode = doc.SelectSingleNode("//appSettings");

            //XmlElement modifyElem = (XmlElement)xNode.SelectSingleNode("//add[@key='" + key + "']");
            //if (modifyElem != null)
            //{
            //    modifyElem.SetAttribute("value", value);
            //}
            //else
            //{
            //    XmlElement addElem = doc.CreateElement("add");
            //    addElem.SetAttribute("key", key);
            //    addElem.SetAttribute("value", value);
            //    xNode.AppendChild(addElem);
            //}

            XmlElement modifyElem = (XmlElement)xNode.SelectSingleNode("//" + key);
            if (modifyElem != null)
            {
                doc["appSettings"][key].InnerText = value;
            }   
            else
            {
                XmlElement addElem = doc.CreateElement(key);
                addElem.InnerText = value;
                xNode.AppendChild(addElem); 
            }

            doc.Save(System.AppDomain.CurrentDomain.BaseDirectory + CONGIG_PATH);
        }
    }
}

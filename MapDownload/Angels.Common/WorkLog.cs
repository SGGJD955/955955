using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Web;
using System.Reflection;

namespace Angels.Common
{
    /// <summary>
    /// 操作日志记录
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WorkLog<T>
    {
        /// <summary>
        /// 操作参数实体及值
        /// </summary>
        /// <param name="t"></param>
        /// <param name="description"></param>
        public WorkLog(T t, string description, string Function)
        {

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(HttpContext.Current.Server.MapPath("/Log/WorkLog.xml"));

            XmlNode root = xmlDoc.SelectSingleNode("DataItem");

            XmlElement xe1 = xmlDoc.CreateElement("WorkLog");

            XmlElement xe1sub1 = xmlDoc.CreateElement("WorkDate");
            xe1sub1.InnerText = DateTime.Now.ToString();
            xe1.AppendChild(xe1sub1);

            XmlElement xe1sub2 = xmlDoc.CreateElement("Function");
            xe1sub2.InnerText = Function;            
            xe1.AppendChild(xe1sub2);

            XmlElement xe1sub3 = xmlDoc.CreateElement("description");
            xe1sub3.InnerText = description;
            xe1.AppendChild(xe1sub3);

            XmlElement xe1sub4 = xmlDoc.CreateElement("ParameterInfo");

            PropertyInfo[] properties = t.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo item in properties)
            {//循环遍历实体，取出字段和字段对应的值

                if (item.GetValue(t, null) != null)
                {
                    XmlElement xe1sub4su1 = xmlDoc.CreateElement("Item");
                    xe1sub4su1.SetAttribute("Field", item.Name);
                    xe1sub4su1.InnerText =  item.GetValue(t, null).ToString();

                    xe1sub4.AppendChild(xe1sub4su1);
                }
            }

            xe1.AppendChild(xe1sub4);


            root.AppendChild(xe1);//添加到<Data>节点中

            xmlDoc.Save(HttpContext.Current.Server.MapPath("/Log/WorkLog.xml"));
        }
    }
}

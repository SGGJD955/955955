using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Web;

using Angels.Application.TicketEntity.Request.Log;
using Angels.Application.TicketEntity;
using Angels.Application.TicketEntity.Common;
using Angels.Common;
using Angels.Application.Data;
using System.Data.OleDb;
using System.IO;

namespace Angels.Application.Service
{
    public class LogService
    {

        public WebApiResult<string> GetList(LogReadRequest Request)
        {
            System.Data.DataTable dt;
            if (Request.GUID =="ALL")
                dt = AccessHelper.ExecuteDataSet(AccessHelper.conn, "select * from Log order by CompleteTime").Tables[0];
            else
                dt = AccessHelper.ExecuteDataSet(AccessHelper.conn, "select * from Log where [GUID]=@GUID", new OleDbParameter("@GUID", Request.GUID)).Tables[0];

            return new WebApiResult<string>() { success = 1, msg = "", results = JsonHelper.ToJson(dt) };
        }

        public WebApiResult<string> GetThreadList(ThreadLogReadRequest Request)
        {
            System.Data.DataTable dt;
            if (Request.GUID == "ALL")
            {
                dt = AccessHelper.ExecuteDataSet(AccessHelper.conn, "select * from ThreadLog order by AddTime").Tables[0];
                dt.Columns.Add("Type");
                dt.Columns.Add("filePath");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["Type"] = AccessHelper.ExecuteDataSet(AccessHelper.conn, "select * from Log order by AddTime").Tables[0].Rows[i]["Type"];
                    dt.Rows[i]["filePath"] = AccessHelper.ExecuteDataSet(AccessHelper.conn, "select * from Log order by AddTime").Tables[0].Rows[i]["SavePath"];
                }
            }

            else
            {
                dt = AccessHelper.ExecuteDataSet(AccessHelper.conn, "select * from ThreadLog where [GUID]=@GUID", new OleDbParameter("@GUID", Request.GUID)).Tables[0];
                dt.Columns.Add("Type");
                dt.Columns.Add("filePath");
                dt.Rows[0]["Type"] = AccessHelper.ExecuteDataSet(AccessHelper.conn, "select * from Log where [GUID]=@GUID", new OleDbParameter("@GUID", Request.GUID)).Tables[0].Rows[0]["Type"];
                dt.Rows[0]["filePath"] = AccessHelper.ExecuteDataSet(AccessHelper.conn, "select * from Log where [GUID]=@GUID", new OleDbParameter("@GUID", Request.GUID)).Tables[0].Rows[0]["SavePath"];
            }

            return new WebApiResult<string>() { success = 1, msg = "", results = JsonHelper.ToJson(dt) };
        }

        public WebApiResult<string> Delete(LogDeleteRequest Request)
        {

            int mm = AccessHelper.ExecuteNonQuery(AccessHelper.conn, "delete from Log where [GUID]=@GUID", new OleDbParameter("@GUID", Request.GUID));
            mm = mm * AccessHelper.ExecuteNonQuery(AccessHelper.conn, "delete from ThreadLog where [GUID]=@GUID", new OleDbParameter("@GUID", Request.GUID));

            if (mm > 0)
            {
                return new WebApiResult<string>() { success = 1, msg = "删除成功" };
            }
            else
            {
                return new WebApiResult<string>() { success = 0, msg = "删除失败，该数据不存在" };
            }
            
        }

        public WebApiResult<string> GetStatusList(LogSelectRequest Request)
        {
            System.Data.DataTable dt = AccessHelper.ExecuteDataSet(AccessHelper.conn, "select * from ThreadLog where [TStatus]=@TStatus", new OleDbParameter("@TStatus", Request.status)).Tables[0];

            return new WebApiResult<string>() { success = 1, msg = "", results = JsonHelper.ToJson(dt) };
        }


        //public WebApiResult<LogEntity> GetList(LogReadRequest Request)
        //{
        //    List<LogEntity> loglist = new List<LogEntity>();

        //    XmlDocument xmlDoc = new XmlDocument();
        //    xmlDoc.Load(HttpContext.Current.Server.MapPath("/Log/Log.xml"));

        //    XmlNodeList nodeList = xmlDoc.SelectSingleNode("DataItem").ChildNodes;

        //    foreach (XmlElement item in nodeList)
        //    {
        //        loglist.Add(new LogEntity() { GUID = item.GetAttribute("GUID"), Type = item.SelectSingleNode("Type").InnerText, Name = item.SelectSingleNode("Name").InnerText, Description = item.SelectSingleNode("Description").InnerText, DataInfo = item.SelectSingleNode("DataInfo").InnerText, ErrorMsg = item.SelectSingleNode("ErrorMsg").InnerText, Parameter = item.SelectSingleNode("Parameter").InnerText, AddTime = item.SelectSingleNode("AddTime").InnerText, Status = item.SelectSingleNode("Status").InnerText });
        //    }

        //    return new WebApiResult<LogEntity>() { success = 1, msg = "", results = JsonHelper.ToJson(loglist) };
        //}


        //public WebApiResult<string> Delete(LogDeleteRequest Request)
        //{
        //    XmlDocument xmlDoc = new XmlDocument();
        //    xmlDoc.Load(HttpContext.Current.Server.MapPath("/Log/Log.xml"));

        //    XmlNode root = xmlDoc.SelectSingleNode("DataItem");

        //    XmlNode nodeList = xmlDoc.SelectSingleNode("/DataItem/Log[@GUID='" + Request.GUID + "']");
        //    if (nodeList != null)
        //    {
        //        root.RemoveChild(nodeList);

        //        xmlDoc.Save(HttpContext.Current.Server.MapPath("/Log/Log.xml"));
        //        return new WebApiResult<string>() { success = 1, msg = "删除成功" };
        //    }
        //    else
        //    {
        //        return new WebApiResult<string>() { success = 0, msg = "删除失败，该数据不存在" };
        //    }

        //}

        /// 清空指定的文件夹，同时删除文件夹
        /// </summary>
        /// <param name="dir"></param>
        public WebApiResult<string> FileDelete(FileDeleteRequest Request)
        {
            try
            {
                string dir = Request.Path;
                foreach (string d in Directory.GetFileSystemEntries(dir))
                {
                    if (File.Exists(d))
                    {
                        FileInfo fi = new FileInfo(d);
                        if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                            fi.Attributes = FileAttributes.Normal;
                        File.Delete(d);//直接删除其中的文件  
                    }
                    else
                    {
                        DeleteFolder(d);////递归删除子文件夹
                        Directory.Delete(d);
                    }
                }

                Directory.Delete(dir);
                return new WebApiResult<string>() { success = 1, msg = "删除成功" };
            }
            catch (Exception ex)
            {
                return new WebApiResult<string>() { success = 0, msg = "删除失败，该文件不存在： " + ex.ToString()};
            }       

        }

        /// 清空指定的文件夹，但不删除文件夹
        /// </summary>
        /// <param name="dir"></param>
        public static void DeleteFolder(string dir)
        {
            foreach (string d in Directory.GetFileSystemEntries(dir))
            {
                if (File.Exists(d))
                {
                    FileInfo fi = new FileInfo(d);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        fi.Attributes = FileAttributes.Normal;
                    File.Delete(d);//直接删除其中的文件  
                }
                else
                {
                    DirectoryInfo d1 = new DirectoryInfo(d);
                    if (d1.GetFiles().Length != 0)
                    {
                        DeleteFolder(d1.FullName);////递归删除子文件夹
                    }
                    Directory.Delete(d);
                }
            }
        }
    }
}

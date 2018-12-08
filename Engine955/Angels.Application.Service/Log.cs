using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Angels.Application.TicketEntity.Common;
using Angels.Application.Data;
using System.Data.OleDb;
using System.Reflection;

namespace Angels.Application.Service
{
    public class Log<T>
    {
        //public Log(LogEntity entity)
        //{
        //    XmlDocument xmlDoc = new XmlDocument();
        //    xmlDoc.Load(HttpContext.Current.Server.MapPath("/Log/Log.xml"));

        //    XmlNode node = xmlDoc.SelectSingleNode("/DataItem/Log[@GUID='" + entity.GUID + "']");
        //    if (node != null)
        //    {//日志存在就进行修改
        //        //node.SelectSingleNode("ErrorMsg").InnerText = entity.ErrorMsg;
        //        //node.SelectSingleNode("Status").InnerText = entity.Status;
        //        //node.SelectSingleNode("Type").InnerText = entity.Type;
        //        //node.SelectSingleNode("ErrorDate").InnerText = DateTime.Now.ToString();

        //        PropertyInfo[] properties = entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        //        foreach (PropertyInfo item in properties)
        //        {//循环遍历实体，取出字段和字段对应的值

        //            if (item.GetValue(entity, null) != null)
        //            {
        //                node.SelectSingleNode(item.Name).InnerText = item.GetValue(entity, null).ToString();
        //            }
        //        }
        //    }
        //    else
        //    {//不存在日志就进行创建
        //        XmlNode root = xmlDoc.SelectSingleNode("DataItem");

        //        XmlElement xe1 = xmlDoc.CreateElement("Log");

        //        xe1.SetAttribute("GUID", entity.GUID);

        //        PropertyInfo[] properties = entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        //        foreach (PropertyInfo item in properties)
        //        {//循环遍历实体，取出字段和字段对应的值

        //            if (item.GetValue(entity, null) != null)
        //            {
        //                XmlElement xe1sub1 = xmlDoc.CreateElement(item.Name);
        //                xe1sub1.InnerText = item.GetValue(entity, null).ToString();
        //                xe1.AppendChild(xe1sub1);
        //            }
        //        }



        //        //XmlElement xe1sub2 = xmlDoc.CreateElement("Status");
        //        //xe1sub2.InnerText = entity.Status;
        //        //xe1.AppendChild(xe1sub2);

        //        //XmlElement xe1sub3 = xmlDoc.CreateElement("Type");
        //        //xe1sub3.InnerText = entity.Type;
        //        //xe1.AppendChild(xe1sub3);

        //        //XmlElement xe1sub4 = xmlDoc.CreateElement("Name");
        //        //xe1sub4.InnerText = entity.Name;
        //        //xe1.AppendChild(xe1sub4);

        //        //XmlElement xe1sub5 = xmlDoc.CreateElement("Description");
        //        //xe1sub5.InnerText = entity.Description;
        //        //xe1.AppendChild(xe1sub5);

        //        //XmlElement xe1sub6 = xmlDoc.CreateElement("DataInfo");
        //        //xe1sub6.InnerText = entity.DataInfo;
        //        //xe1.AppendChild(xe1sub6);

        //        //XmlElement xe1sub7 = xmlDoc.CreateElement("ErrorMsg");
        //        //xe1sub7.InnerText = entity.ErrorMsg;
        //        //xe1.AppendChild(xe1sub7);

        //        //XmlElement xe1sub9 = xmlDoc.CreateElement("ErrorDate");
        //        //xe1sub9.InnerText = entity.ErrorMsg;
        //        //xe1.AppendChild(xe1sub9);

        //        //XmlElement xe1sub8 = xmlDoc.CreateElement("Parameter");

        //        //PropertyInfo[] properties = t.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

        //        //string datajson = "{";
        //        //foreach (PropertyInfo item in properties)
        //        //{//循环遍历实体，取出字段和字段对应的值

        //        //    if (item.GetValue(t, null) != null)
        //        //    {
        //        //        //XmlElement xe1sub4su1 = xmlDoc.CreateElement("Item");
        //        //        //xe1sub4su1.SetAttribute("Field", item.Name);
        //        //        //xe1sub4su1.InnerText = item.GetValue(t, null).ToString();
        //        //        //xe1sub8.AppendChild(xe1sub4su1);
        //        //        datajson += (datajson == "{" ? "" : ",") + "\"" + item.Name + "\":\"" + item.GetValue(t, null).ToString() + "\"";
        //        //    }
        //        //}
        //        //datajson += "}";

        //        //xe1sub8.InnerText = datajson;

        //        //xe1.AppendChild(xe1sub8);

        //        root.AppendChild(xe1);//添加到<Data>节点中
        //    }

        //    xmlDoc.Save(HttpContext.Current.Server.MapPath("/Log/Log.xml"));
        //}

        /// <summary>
        /// 将日志存到数据库中
        /// </summary>
        /// <param name="entity"></param>
        public Log(TaskLogEntity entity)
        {
            try
            {
                System.Data.DataTable dt = AccessHelper.ExecuteDataSet(AccessHelper.conn, "select * from Log where [GUID]=@GUID", new OleDbParameter("@GUID", entity.GUID)).Tables[0];
                if (dt.Rows.Count > 0)
                {
                    int mm = AccessHelper.ExecuteNonQuery(AccessHelper.conn, "update Log set Log.[Type]=@Type,[Name]=@Name,[Description]=@Description,[DataInfo]=@DataInfo,[ErrorMsg]=@ErrorMsg,[Parameter]=@Parameter,[AddTime]=@AddTime,[ErrorDate]=@ErrorDate,[CompleteTime]=@CompleteTime,[Status]=@Status,[Count]=@Count,[Current]=@Current,[SavePath]=@SavePath where [GUID]=@GUID ", new OleDbParameter("@Type", entity.Type), new OleDbParameter("@Name", entity.Name), new OleDbParameter("@Description", entity.Description), new OleDbParameter("@DataInfo", entity.DataInfo), new OleDbParameter("@ErrorMsg", entity.ErrorMsg), new OleDbParameter("@Parameter", entity.Parameter), new OleDbParameter("AddTime", entity.AddTime), new OleDbParameter("@ErrorDate", entity.ErrorDate), new OleDbParameter("@CompleteTime", entity.CompleteTime), new OleDbParameter("@Status", entity.Status), new OleDbParameter("@Count", entity.Count), new OleDbParameter("@Current", entity.Current), new OleDbParameter("@SavePath", entity.SavePath), new OleDbParameter("@GUID", entity.GUID));
                }
                else
                {
                    int mm = AccessHelper.ExecuteNonQuery(AccessHelper.conn, "insert into Log([GUID],[Type],[Name],[Description],[DataInfo],[ErrorMsg],[Parameter],[AddTime],[ErrorDate],[CompleteTime],[Status],[Count],[Current],[SavePath]) values(@GUID,@Type,@Name,@Description,@DataInfo,@ErrorMsg,@Parameter,@AddTime,@ErrorDate,@CompleteTime,@Status,@Count,@Current,@SavePath)", new OleDbParameter("@GUID", entity.GUID), new OleDbParameter("@Type", entity.Type), new OleDbParameter("@Name", entity.Name), new OleDbParameter("@Description", entity.Description), new OleDbParameter("@DataInfo", entity.DataInfo), new OleDbParameter("@ErrorMsg", entity.ErrorMsg), new OleDbParameter("@Parameter", entity.Parameter), new OleDbParameter("AddTime", entity.AddTime), new OleDbParameter("@ErrorDate", entity.ErrorDate), new OleDbParameter("@CompleteTime", entity.CompleteTime), new OleDbParameter("@Status", entity.Status), new OleDbParameter("@Count", entity.Count), new OleDbParameter("@Current", entity.Current), new OleDbParameter("@SavePath", entity.SavePath));
                }
            }
            catch (Exception ex)
            { 
            
            }
        }

        public Log(ThreadTaskLogEntity entity)
        {
            try
            {
                System.Data.DataTable dt = AccessHelper.ExecuteDataSet(AccessHelper.conn, "select * from ThreadLog where [GUID]=@GUID", new OleDbParameter("@GUID", entity.GUID)).Tables[0];
                if (dt.Rows.Count > 0)
                {
                    int mm = AccessHelper.ExecuteNonQuery(AccessHelper.conn, "update ThreadLog set ThreadLog.[TaskLog_GUID]=@TaskLog_GUID,[Current]=@Current,[Total]=@Total,[Status]=@Status,[Current_loc]=@Current_loc,[ErrorMsg]=@ErrorMsg,[AddTime]=@AddTime,[IsPaused]=@IsPaused,[TName]=@TName,[TStatus]=@TStatus,[Parameter]=@Parameter,[URL]=@URL where [GUID]=@GUID ", new OleDbParameter("@TaskLog_GUID", entity.TaskLog_GUID), new OleDbParameter("@Current", entity.Current), new OleDbParameter("@Total", entity.Total), new OleDbParameter("@Status", entity.Status), new OleDbParameter("@Current_loc", entity.Current_loc),  new OleDbParameter("@ErrorMsg", entity.ErrorMsg), new OleDbParameter("@AddTime", entity.AddTime),new OleDbParameter("@IsPaused", entity.IsPaused), new OleDbParameter("@TName", entity.TName), new OleDbParameter("@TStatus", entity.TStatus), new OleDbParameter("@Parameter", entity.Parameter), new OleDbParameter("@URL", entity.URL), new OleDbParameter("@GUID",entity.GUID));
                }
                else
                {
                    int mm = AccessHelper.ExecuteNonQuery(AccessHelper.conn, "insert into ThreadLog([GUID],[TaskLog_GUID],[Current],[Total],[Status],[Current_loc],[ErrorMsg],[AddTime],[IsPaused],[TName],[TStatus],[Parameter],[URL]) values(@GUID,@TaskLog_GUID,@Current,@Total,@Status,@Current_loc,@ErrorMsg,@AddTime,@IsPaused,@TName,@TStatus,@Parameter,@URL)", new OleDbParameter("@GUID", entity.GUID), new OleDbParameter("@TaskLog_GUID", entity.TaskLog_GUID), new OleDbParameter("@Current", entity.Current), new OleDbParameter("@Total", entity.Total), new OleDbParameter("@Status", entity.Status), new OleDbParameter("@Current_loc", entity.Current_loc), new OleDbParameter("@ErrorMsg", entity.ErrorMsg), new OleDbParameter("@AddTime", entity.AddTime), new OleDbParameter("@IsPaused",entity.IsPaused), new OleDbParameter("@TName", entity.TName), new OleDbParameter("@TStatus", entity.TStatus),new OleDbParameter("@Parameter", entity.Parameter), new OleDbParameter("@URL", entity.URL));
                }
            }
            catch (Exception ex)
            {

            }
        }

        static public TaskLogEntity GetLogEntity(string GUID)
        {
            TaskLogEntity entity = new TaskLogEntity();
            //try
            //{
            //    XmlDocument xmlDoc = new XmlDocument();
            //    xmlDoc.Load(HttpContext.Current.Server.MapPath("/Log/Log.xml"));

            //    XmlNode node = xmlDoc.SelectSingleNode("/DataItem/Log[@GUID='" + GUID + "']");
            //    if (node != null)
            //    {//日志存在就进行修改
            //        //node.SelectSingleNode("ErrorMsg").InnerText = entity.ErrorMsg;
            //        //node.SelectSingleNode("Status").InnerText = entity.Status;
            //        //node.SelectSingleNode("Type").InnerText = entity.Type;
            //        //node.SelectSingleNode("ErrorDate").InnerText = DateTime.Now.ToString();

            //        PropertyInfo[] properties = entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            //        foreach (PropertyInfo item in properties)
            //        {//循环遍历实体，取出字段和字段对应的值

            //            if (item.GetValue(entity, null) != null)
            //            {
            //                item.SetValue(entity, node.SelectSingleNode(item.Name).InnerText, null);
            //            }
            //        }

            //    }
            //}
            //catch (Exception ex)
            //{

            //}
            //return entity;

            try
            {
                System.Data.DataTable dt = AccessHelper.ExecuteDataSet(AccessHelper.conn, "select * from Log where [GUID]=@GUID", new OleDbParameter("@GUID", GUID)).Tables[0];

                if (dt.Rows.Count > 0)
                {
                    PropertyInfo[] properties = entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    foreach (PropertyInfo item in properties)
                    {//循环遍历实体，取出字段和字段对应的值

                        if (item.GetValue(entity, null) != null)
                        {
                            item.SetValue(entity, dt.Rows[0][item.Name].ToString(), null);
                        }
                    }


                    return entity;
                }


            }
            catch (Exception ex)
            { 
            
            }

            return null;
        }

        static public ThreadTaskLogEntity GetThreadLogEntity(string GUID)
        {
            ThreadTaskLogEntity entity = new ThreadTaskLogEntity();

            try
            {
                System.Data.DataTable dt = AccessHelper.ExecuteDataSet(AccessHelper.conn, "select * from ThreadLog where [GUID]=@GUID", new OleDbParameter("@GUID", GUID)).Tables[0];

                if (dt.Rows.Count > 0)
                {
                    PropertyInfo[] properties = entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    foreach (PropertyInfo item in properties)
                    {//循环遍历实体，取出字段和字段对应的值

                        if (item.GetValue(entity, null) != null)
                        {
                            item.SetValue(entity, dt.Rows[0][item.Name], null);
                        }
                    }

                    return entity;
                }


            }
            catch (Exception ex)
            {

            }

            return null;
        }
    }
}

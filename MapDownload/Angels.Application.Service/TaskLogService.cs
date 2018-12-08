using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.OleDb;
using System.Reflection;

using Angels.Application.TicketEntity.Common;
using Angels.Application.Data;

namespace Angels.Application.Service
{
    public class TaskLogService
    {
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public DataTable GetList(string where)
        {
            try
            {
                DataTable dt = AccessHelper.ExecuteDataSet(AccessHelper.conn, "select * from TaskLogEntity where " + where).Tables[0];

                return dt;
            }
            catch (Exception ex)
            { 
            
            }

            return null;
        }

        /// <summary>
        /// 获取实体
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns></returns>
        public TaskLogEntity GetEntity(string GUID)
        {
            try
            {

                TaskLogEntity entity = new TaskLogEntity();

                DataTable dt = AccessHelper.ExecuteDataSet(AccessHelper.conn, "select * from TaskLogEntity where [GUID]=@GUID", new OleDbParameter("@GUID", GUID)).Tables[0];

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

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns></returns>
        public int Remove(string GUID)
        {
            try
            {
                int mm = AccessHelper.ExecuteNonQuery(AccessHelper.conn, "delete from TaskLogEntity where [GUID]=@GUID", new OleDbParameter("@GUID", GUID));

                return mm;
            }
            catch (Exception ex)
            { 
            
            }

            return 0;
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="GUID"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int FormSave(TaskLogEntity entity)
        {
            try
            {
                List<OleDbParameter> parameter = new List<OleDbParameter>();

                string SqlStr = "";

                if (AccessHelper.ExecuteDataSet(AccessHelper.conn, "select * from " + entity.GetType().Name + " where [GUID]=@GUID", new OleDbParameter("@GUID", entity.GUID)).Tables[0].Rows.Count > 0)
                {
                    SqlStr = "update " + entity.GetType().Name + " set ";

                    PropertyInfo[] properties = entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    foreach (PropertyInfo item in properties)
                    {//循环遍历实体，取出字段和字段对应的值

                        if (item.GetValue(entity, null) != null)
                        {
                            if (item.Name != "GUID")
                            {
                                SqlStr += (SqlStr.EndsWith("set ") ? "" : ",") + "[" + item.Name + "]=@" + item.Name;
                                parameter.Add(new OleDbParameter("@" + item.Name, item.GetValue(entity, null).ToString()));
                            }
                        }
                    }

                    SqlStr += " where [GUID]=@GUID";

                }
                else
                {
                    string Field = "", Variable = "";

                    PropertyInfo[] properties = entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    foreach (PropertyInfo item in properties)
                    {//循环遍历实体，取出字段和字段对应的值

                        if (item.GetValue(entity, null) != null)
                        {
                            Field += (Field == "" ? "" : ",") + "[" + item.Name + "]";
                            Variable += (Variable == "" ? "" : ",") + "@" + item.Name;
                            parameter.Add(new OleDbParameter("@" + item.Name, item.GetValue(entity, null).ToString()));
                        }
                    }

                    SqlStr = "insert into " + entity.GetType().Name + "(" + Field + ") values(" + Variable + ")";
                }

                int mm = AccessHelper.ExecuteNonQuery(AccessHelper.conn, SqlStr, parameter.ToArray());

                return mm;
            }
            catch (Exception ex)
            { 
            
            }

            return 0;
        }
        
    }
}

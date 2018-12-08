using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;
using DBAccess;
namespace Angels.Application.Data
{
    public class AccessSqlHelper
    {

        public AccessSqlHelper()
        {
            
        }

        public int execCommand(string connectionString,string strCommand)
        {
            MySqlAccess helper = new MySqlAccess(connectionString);

            return helper.execCommand(strCommand);
        }



        public int execCommand(string connectionString,string procName, IDataParameter[] paras)
        {
            MySqlAccess helper = new MySqlAccess(connectionString);

            return helper.execCommand(procName, paras);
        }

        public IDataReader getDataReader(string connectionString, string strCommand)
        {
            MySqlAccess helper = new MySqlAccess(connectionString);

            return helper.getDataReader(strCommand);
        }

        public IDataReader getDataReader(string connectionString, string procName, IDataParameter[] paras)
        {
            MySqlAccess helper = new MySqlAccess(connectionString);

            return helper.getDataReader(procName, paras);
        }

        public DataSet getDataSet(string connectionString, string strCommand)
        {
            MySqlAccess helper = new MySqlAccess(connectionString);

            return helper.getDataSet(strCommand);
        }

        public DataSet getDataSet(string connectionString, string procName, IDataParameter[] paras)
        {
            MySqlAccess helper = new MySqlAccess(connectionString);

            return helper.getDataSet(procName, paras);
        }
        public DataTable getDataTable(string connectionString, string strCommand)
        {
            MySqlAccess helper = new MySqlAccess(connectionString);

            return helper.getDataTable(strCommand);
        }
        public DataTable getDataTable(string connectionString, string procName, IDataParameter[] paras)
        {
            MySqlAccess helper = new MySqlAccess(connectionString);

            return helper.getDataTable(procName, paras);
        }
        public List<T> getDataTableForObj<T>(string connectionString, string strCommand)
        {
            MySqlAccess helper = new MySqlAccess(connectionString);

            return helper.getDataTableForObj<T>(strCommand);
        }
        public List<T> getDataTableForObj<T>(string connectionString, string procName, IDataParameter[] paras)
        {
            MySqlAccess helper = new MySqlAccess(connectionString);

            return helper.getDataTableForObj<T>(procName, paras);
        }
        public List<T> getObjByTable<T>(DataTable dt)
        {
            MySqlAccess helper = new MySqlAccess("");

            return helper.getObjByTable<T>(dt);
        }
        public T getSinggleObj<T>(string connectionString, string strCommand)
        {
            MySqlAccess helper = new MySqlAccess(connectionString);

            return helper.getSinggleObj<T>(strCommand);
        }
        public T getSinggleObj<T>(string connectionString, string procName, IDataParameter[] paras)
        {
            MySqlAccess helper = new MySqlAccess(connectionString);

            return helper.getSinggleObj<T>(procName, paras);
        }
    }
}

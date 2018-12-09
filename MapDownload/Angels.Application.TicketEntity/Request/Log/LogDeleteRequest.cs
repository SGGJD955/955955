using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Angels.Application.TicketEntity.Request.Log
{
    public class LogDeleteRequest
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public string GUID { get; set; }
    }
    public class FileDeleteRequest
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public string Path { get; set; }
    }

    public class OpenFolderRequest
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public string Path { get; set; }
    }
}

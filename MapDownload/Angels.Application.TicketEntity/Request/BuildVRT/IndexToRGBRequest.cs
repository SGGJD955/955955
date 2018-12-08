using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Angels.Application.TicketEntity.Request.BuildVRT
{
    /// <summary>
    /// index转换RGB请求参数
    /// </summary>
    public class IndexToRGBRequest
    {
        /// <summary>
        /// 存放瓦片的文件夹路径
        /// </summary>
        public string path { get; set; }

        /// <summary>
        /// 请求唯一ID
        /// </summary>
        public string GUID { get; set; }
    }
}

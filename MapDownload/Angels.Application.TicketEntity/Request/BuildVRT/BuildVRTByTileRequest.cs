using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Angels.Application.TicketEntity.Request.BuildVRT
{
    /// <summary>
    /// 生成VRT请求参数
    /// </summary>
    public class BuildVRTByTileRequest
    {
        /// <summary>
        /// 默认瓦片路径文件夹
        /// </summary>
        public string defaultfolderpath { get; set; }

        /// <summary>
        /// 请求唯一ID
        /// </summary>
        public string GUID { get; set; }
    }
}

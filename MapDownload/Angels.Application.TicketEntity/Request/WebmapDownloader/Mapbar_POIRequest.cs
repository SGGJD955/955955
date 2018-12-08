using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Angels.Application.TicketEntity.Request.WebmapDownloader
{
    /// <summary>
    /// 图吧网络地图下载器请求参数
    /// </summary>
    public class Mapbar_POIRequest
    {
        /// <summary>
        /// 城市拼音（如wuhan）
        /// </summary>
        public string CityName { get; set; }
        /// <summary>
        /// 城市中文名
        /// </summary>
        public string CityName_zh { get; set; }

        /// <summary>
        /// 唯一ID
        /// </summary>
        public string GUID { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        public string TName { get; set; }

        /// <summary>
        /// 线程数量
        /// </summary>
        public int TaskCount { get; set; }
        /// <summary>
        /// 接口链接
        /// </summary>
        public string URL { get; set; }
    }
}

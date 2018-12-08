using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Angels.Application.TicketEntity.Request.WebmapDownloader
{
    /// <summary>
    /// 百度POI请求参数
    /// </summary>
    public class Baidumap_POIRequest
    {
        /// <summary>
        /// 城市中文全名（如武汉市）
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 请求任务唯一ID
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

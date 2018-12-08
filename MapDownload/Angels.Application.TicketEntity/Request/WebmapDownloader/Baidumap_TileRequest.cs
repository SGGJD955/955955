using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Angels.Application.TicketEntity.Common.WebmapDownloader;

namespace Angels.Application.TicketEntity.Request.WebmapDownloader
{
    /// <summary>
    /// 百度瓦片请求参数
    /// </summary>
    public class Baidumap_TileRequest
    {
        /// <summary>
        /// 开始坐标39.865188, 116.3877
        /// </summary>
        public LatLngPoint startcoord { get; set; }

        /// <summary>
        /// 结束坐标39.979655, 116.45749
        /// </summary>
        public LatLngPoint endcoord { get; set; }

        /// <summary>
        /// 唯一请求ID
        /// </summary>
        public string GUID { get; set; }

        /// <summary>
        /// 保存路径E:\\BaiduMap\\Beijing\\DongCheng\\ALL\\tiles
        /// </summary>
        public string SavePathText { get; set; }

        /// <summary>
        /// 并行线程数
        /// </summary>
        public int TaskCount { get; set; }
        /// <summary>
        /// 任务名称
        /// </summary>
        public string TName { get; set; }
        /// <summary>
        /// 接口链接
        /// </summary>
        public string URL { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Angels.Application.TicketEntity.Common.WebmapDownloader;

namespace Angels.Application.TicketEntity.Request.WebmapDownloader
{
    /// <summary>
    /// 百度建筑瓦片请求参数
    /// </summary>
    public class Baidumap_BuildingTileRequest
    {
        /// <summary>
        /// 开始坐标--恩施--29.120116, 108.374308
        /// </summary>
        public LatLngPoint startcoord { get; set; }
        /// <summary>
        /// 结束坐标--恩施--31.402847, 110.649141
        /// </summary>
        public LatLngPoint endcoord { get; set; }

        /// <summary>
        /// 恩施URL----http://api{0}.map.bdimg.com/customimage/tile?&x={1}&y={2}&z={3}&udt=20171214&scale=1&ak=8d6c8b8f3749aed6b1aff3aad6f40e37&styles=t%3Aadministrative%7Ce%3Aall%7Cv%3Aoff%2Ct%3Apoi%7Ce%3Aall%7Cv%3Aoff%2Ct%3Aroad%7Ce%3Aall%7Cv%3Aoff%2Ct%3Amanmade%7Ce%3Aall%7Cv%3Aoff%2Ct%3Abuilding%7Ce%3Aall%7Cv%3Aoff
        /// </summary>
        public string originlink { get; set; }

        public string GUID { get; set; }

        /// <summary>
        /// 保存路径E:\\BaiduMap20171214\\GreenWater\\tiles
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

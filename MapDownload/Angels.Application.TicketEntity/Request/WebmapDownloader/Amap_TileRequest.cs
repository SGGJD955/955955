using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Angels.Application.TicketEntity.Common.WebmapDownloader;

namespace Angels.Application.TicketEntity.Request.WebmapDownloader
{
    /// <summary>
    /// 高德瓦片请求参数
    /// </summary>
    public class Amap_TileRequest
    {
        ///// <summary>
        ///// 开始坐标--武汉地区坐标范围信息--29.972313, 113.707018
        ///// </summary>
        //public LatLngPoint startcoord { get; set; }
        ///// <summary>
        ///// 结束坐标--武汉地区坐标范围信息--31.367044, 115.087233
        ///// </summary>
        //public LatLngPoint endcoord { get; set; }

        /// <summary>
        /// 左坐标
        /// </summary>
        public double LefttextBox { get; set; }
        /// <summary>
        /// 底部坐标
        /// </summary>
        public double BottomtextBox { get; set; }
        /// <summary>
        /// 右部坐标
        /// </summary>
        public double RighttextBox { get; set; }
        /// <summary>
        /// 头部坐标
        /// </summary>
        public double UptextBox { get; set; }

        /// <summary>
        /// 请求唯一ID
        /// </summary>
        public string GUID { get;set; }

        /// <summary>
        /// 保存路径 "E:\\AMap20171110\\tiles";
        /// </summary>
        public string SavePathText { get; set; }

        /// <summary>
        /// 图层信息拼接好的字符串
        /// </summary>
        public string LayerStr { get; set; }

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

        /// <summary>
        /// 地图层级列表数组
        /// </summary>
        public int[] LevelList { get; set; }
    }
}

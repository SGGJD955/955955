using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Angels.Application.TicketEntity.Request.ImageMosaic
{
    /// <summary>
    /// 百度瓦片拼接请求参数
    /// </summary>
    public class BaiduTileSplicingResult
    {
        /// <summary>
        /// 文件路径（F:\\BaiduMap20171214\\GreenWater\\tiles\\17）
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 保存路径（F:\\BaiduMap20171214\\GreenWater\\bigtiles\\17\\）
        /// </summary>
        public string SavePath { get; set; }

        public string GUID { get; set; }

        /// <summary>
        /// 拼接尺寸
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// 坐标系统
        /// </summary>
        public string crstype { get; set; }

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

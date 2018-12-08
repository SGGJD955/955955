using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Angels.Application.TicketEntity.Request.MapFeatureDownload
{
    /// <summary>
    /// 百度地图
    /// </summary>
    public class BaiduMapFeatureDownloadResult
    {
        /// <summary>
        /// 城市中文全名（如北京市）
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 图层名称(如道路等图层)
        /// </summary>
        public string LayerName { get; set; }

        /// <summary>
        /// 创建数据源文件路径
        /// </summary>
        public string SavePathText { get; set; }        

        /// <summary>
        /// 请求唯一ID
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

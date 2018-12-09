using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Angels.Application.TicketEntity.Request.Log
{
    public class LogReadRequest
    {
        /// <summary>
        /// 读取条件
        /// </summary>
        public string GUID { get; set; }
    }

    public class ThreadLogReadRequest
    {
        /// <summary>
        /// 读取条件
        /// </summary>
        public string GUID { get; set; }
    }

    public class LogSelectRequest
    {
        /// <summary>
        /// 读取条件
        /// </summary>
        public int status { get; set; }
    }

    //public class ImportCityIdRequest
    //{
    //    /// <summary>
    //    /// 城市中文名
    //    /// </summary>
    //    public string City { get; set; }
    //    /// <summary>
    //    /// Id
    //    /// </summary>
    //    public string Id { get; set; }
    //    /// <summary>
    //    /// pId
    //    /// </summary>
    //    public string pId { get; set; }
    //    /// <summary>
    //    /// 地图类别（百度maptype=0, 高德maptype=1)
    //    /// </summary>
    //    public int maptype { get; set; }
    //}

    //public class ImportCityNameRequest
    //{
    //    /// <summary>
    //    /// 城市中文名
    //    /// </summary>
    //    public string City { get; set; }
    //    /// <summary>
    //    /// 城市拼音
    //    /// </summary>
    //    public string Name { get; set; }
    //}

    public class ImportCityInformRequest
    {
        /// <summary>
        /// 城市中文名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 百度Id
        /// </summary>
        public string baiduId { get; set; }
        /// <summary>
        /// 百度pId
        /// </summary>
        public string baidupId { get; set; }
        /// <summary>
        /// 高德Id
        /// </summary>
        public string gaodeId { get; set; }
        /// <summary>
        /// 城市拼音
        /// </summary>
        public string pinyin { get; set; }
    }

    public class GetCityInformRequest
    {
        
    }

    public class GetCityNameRequest
    {

    }

    public class DeleteCityInformRequest
    {
        /// <summary>
        /// Name组
        /// </summary>
        public string[] Names { get; set; }
        /// <summary>
        /// 表格（百度type=0, 高德type=1,图吧type=2)
        /// </summary>
        //public int type { get; set; }
    }

    public class Hello
    {

    }
}

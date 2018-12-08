using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Angels.Application.TicketEntity.Request.ImageMosaic
{
    /// <summary>
    /// 高德的图二值化请求参数
    /// </summary>
    public class GaodeErzhihuaRequest
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }

        /// 是否为道路图层
        /// </summary>
        public bool IsRoad { get; set; }



    }
}

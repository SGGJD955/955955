using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Angels.Application.TicketEntity
{
    public class WebApiResult<T>
    {
        public WebApiResult()
        {
            success = 2;

        }

        /// <summary>
        /// 請求及執行是否成功
        /// </summary>
        public int success { get; set; }

        /// <summary>
        /// 返回提示内容
        /// </summary>
        public string msg { get; set; }

        /// <summary>
        /// 该属性包含返回的结果列表
        /// </summary>
        public string results { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

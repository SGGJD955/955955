using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Angels.Application.TicketEntity.Common
{
    public class ThreadTaskLogEntity
    {
        public ThreadTaskLogEntity()
        {
            this.GUID = Guid.NewGuid().ToString();
            this.TaskLog_GUID = Guid.NewGuid().ToString();
            this.AddTime = DateTime.Now.ToString();

            this.Current = 0;
            this.Total = 0;
                       
            this.ErrorMsg = "";
            this.Status = "";

            this.TName = "";
            this.TStatus = 0;

            this.Current_loc = "";

            this.IsPaused = false;

            this.Parameter = "";
            this.URL = "";
            
        }

        /// <summary>
        /// 唯一ID
        /// </summary>
        public string GUID { get; set; }
        /// <summary>
        /// 任务ID
        /// </summary>
        public string TaskLog_GUID { get; set; }
        /// <summary>
        /// 当前执行条数
        /// </summary>
        public int Current { get; set; }
        /// <summary>
        /// 总计数量
        /// </summary>
        public int Total { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 当前执行位置
        /// </summary>
        public string Current_loc { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMsg { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        public string AddTime { get; set; }
        

        public bool IsPaused { get; set; }
        public string TName { get; set; }

        /// <summary>
        /// 状态代号（0初始 1进行中 2已完成 3错误 4暂停）
        /// </summary>
        public int TStatus { get; set; }

        public string Parameter { get; set; }
        public string URL { get; set; }
    }
}

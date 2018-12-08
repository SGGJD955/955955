using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Angels.Application.TicketEntity.Common
{
    /// <summary>
    /// 日志
    /// </summary>
    public class TaskLogEntity
    {
        public TaskLogEntity()
        {
            this.GUID = Guid.NewGuid().ToString();
            this.AddTime = DateTime.Now.ToString();
            this.ErrorDate = DateTime.Now.ToString();
            this.CompleteTime = DateTime.Now.ToString();
         
            this.Type = "";
            this.Name = "";
            this.Description = "";
            this.DataInfo = "";
            this.ErrorMsg = "";
            this.Parameter = "";
            this.Status = "";
            this.SavePath = "";

            this.Count = 0;

        }
        /// <summary>
        /// 唯一主键
        /// </summary>
        public string GUID { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 名称（功能名称等）
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 数据详情（JSON数据格式）
        /// </summary>
        public string DataInfo { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMsg { get; set; }

        /// <summary>
        /// 参数实体
        /// </summary>
        public string Parameter { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        public string AddTime { get; set; }

        /// <summary>
        /// 报错时间
        /// </summary>
        public string ErrorDate { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public string CompleteTime { get; set; }

        /// <summary>
        /// 未完成、进行中、错误、已完成
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 总共多少条
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 当前多少条
        /// </summary>
        public int Current { get; set; }

        /// <summary>
        /// 存储地址
        /// </summary>
        public string SavePath { get; set; }

    }
}

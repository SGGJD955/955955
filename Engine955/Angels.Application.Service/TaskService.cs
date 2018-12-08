using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Angels.Application.TicketEntity.Request.WebmapDownloader;
using Angels.Application.TicketEntity;

namespace Angels.Application.Service
{
    class TaskService
    {
        public WebApiResult<string> GetSchedule(ScheduleDataRequest request)
        {
            try
            {
                double current = Log<Amap_TileRequest>.GetThreadLogEntity(request.Key).Current;
                double total = Log<Amap_TileRequest>.GetThreadLogEntity(request.Key).Total;
                int res = (int)(current / total * 100);
                return new WebApiResult<string> { success = 1, msg = res.ToString() };

            }
            catch(Exception ex)
            {
                return null;// new WebApiResult<string> { success = 0, msg = "错误" };
            }
        }

        public WebApiResult<string> Pause<T>(TaskStop request)
        {
            var entity = Log<T>.GetThreadLogEntity(request.GUID);
            if (entity == null) return new WebApiResult<string> { success = 0, msg = "该任务不存在" };
            if (entity.Status == "已完成") return new WebApiResult<string> { success = 0, msg = "该任务已完成" };
            entity.IsPaused = true;
            entity.TStatus = 4;
            new Log<T>(entity);
            return new WebApiResult<string>() { success = 1, msg = "暂停成功" };
        }

    }
}

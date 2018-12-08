using System;
using System.Collections.Generic;

namespace Angels.Application.TicketEntity
{
    /// <summary>
    /// 创建票据响应数据实体
    /// </summary>
    public class TicketResponse
    {
        public int TicketId { get; set; }
        public int TableNumber { get; set; }
        public int ServerId { get; set; }
        public List<OrderResponse> Orders { get; set; }
        public DateTime Timestamp { get; set; }
    }
    /// <summary>
    /// 票据所属订单
    /// </summary>
    public class OrderResponse
    {
        public int OrderId { get; set; }
    }
}

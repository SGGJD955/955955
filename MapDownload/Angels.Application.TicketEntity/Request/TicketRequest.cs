
using ServiceStack;
namespace Angels.Application.TicketEntity
{
    /// <summary>
    /// 创建票据 请求实体
    /// </summary>
    public class TicketRequest //: IReturn<TicketResponse>
    {
        public int TicketId { get; set; }
        public int TableNumber { get; set; }
        public int ServerId { get; set; }
    }
    public class TicketDeleteRequest
    {
        public int TicketId { get; set; }
    }
}

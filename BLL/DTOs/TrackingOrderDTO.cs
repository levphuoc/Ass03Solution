

using DataAccessLayer.Enum;

namespace BLL.DTOs
{
    public class TrackingOrderDTO
    {
        public int TracingOrderId { get; set; }
        public OrderStatus Status { get; set; }
        public int MemberId { get; set; }
        public int OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}


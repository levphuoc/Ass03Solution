using DataAccessLayer.Enum;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Entities
{
    public class TracingOrder
    {
        [Key]
        public int TracingOrderId { get; set; }

        public OrderStatus Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now; 

        [Required]
        public int MemberId { get; set; }

        public string MemberName { get; set; } = string.Empty;


        [ForeignKey("Order")]
        public int OrderId { get; set; }

        public virtual Order Order { get; set; }
    }
}

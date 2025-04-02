using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Entities
{
    public class TracingOrder
    {
        [Key]
        public int TracingOrderId { get; set; }

        [Required, MaxLength(50)]
        public string OrderStatus { get; set; }

        [Required]
        public string MemberId { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }

        public virtual Order Order { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Entities
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required, MaxLength(40)]
        public string ProductName { get; set; }

        [Required, MaxLength(20)]
        public string Weight { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public int UnitsInStock { get; set; }

        public Category Category { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs
{
    public class OrderDTO
    {
        public int OrderId { get; set; }
        [Required(ErrorMessage = "MemberId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "MemberId must be greater than 0.")]
        public int MemberId { get; set; } = 1; // Set giá trị mặc định

        [Required(ErrorMessage = "OrderDate is required.")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "RequiredDate is required.")]
        public DateTime RequiredDate { get; set; } = DateTime.Now.AddDays(7);

        [Required(ErrorMessage = "ShippedDate is required.")]
        public DateTime ShippedDate { get; set; } = DateTime.Now.AddDays(3);

        [Required(ErrorMessage = "Freight is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Freight must be greater than 0.")]
        public decimal Freight { get; set; } = 1m;

        
        public List<OrderItemDTO> OrderItems { get; set; } = new List<OrderItemDTO>();
    }
}

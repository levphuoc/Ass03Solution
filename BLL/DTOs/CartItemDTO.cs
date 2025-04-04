using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
    public class CartItemDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total
        {
            get
            {
                return UnitPrice * Quantity;
            }
        }
        public decimal RecomputeTotal()
        {
            return UnitPrice * Quantity;
        }
        public string CategoryName { get; set; }
    }

    public class CartDTO
    {
        public List<CartItemDTO> Items { get; set; } = new List<CartItemDTO>();
        public decimal TotalAmount
        {
            get
            {
                return Items.Sum(item => item.UnitPrice * item.Quantity);
            }
        }
        public int TotalItems => Items.Sum(item => item.Quantity);
        public string ErrorMessage { get; set; }
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    }
} 
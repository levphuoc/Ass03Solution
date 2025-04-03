using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
    public class ProductDTO
    {
        public int ProductId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required, MaxLength(40)]
        public string ProductName { get; set; }

        [MaxLength(20), RegularExpression(@"^[0-9]+[a-zA-Z]*$", ErrorMessage = "Weight must be a positive number followed by optional unit")]
        public string Weight { get; set; }

        [Required, Range(0, double.MaxValue, ErrorMessage = "Unit Price cannot be negative")]
        public decimal UnitPrice { get; set; }

        [Required, Range(0, int.MaxValue, ErrorMessage = "Units In Stock cannot be negative")]
        public int UnitsInStock { get; set; }

        [MaxLength(255)]
        public string? UrlImage { get; set; }

        public string? CategoryName { get; set; }
    }

    public class CreateProductDTO
    {
        [Required]
        public int CategoryId { get; set; }

        [Required, MaxLength(40)]
        public string ProductName { get; set; }

        [MaxLength(20), RegularExpression(@"^[0-9]+[a-zA-Z]*$", ErrorMessage = "Weight must be a positive number followed by optional unit")]
        public string Weight { get; set; }

        [Required, Range(0, double.MaxValue, ErrorMessage = "Unit Price cannot be negative")]
        public decimal UnitPrice { get; set; }

        [Required, Range(0, int.MaxValue, ErrorMessage = "Units In Stock cannot be negative")]
        public int UnitsInStock { get; set; }

        [MaxLength(255)]
        public string? UrlImage { get; set; }
    }

    public class UpdateProductDTO
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required, MaxLength(40)]
        public string ProductName { get; set; }

        [MaxLength(20), RegularExpression(@"^[0-9]+[a-zA-Z]*$", ErrorMessage = "Weight must be a positive number followed by optional unit")]
        public string Weight { get; set; }

        [Required, Range(0, double.MaxValue, ErrorMessage = "Unit Price cannot be negative")]
        public decimal UnitPrice { get; set; }

        [Required, Range(0, int.MaxValue, ErrorMessage = "Units In Stock cannot be negative")]
        public int UnitsInStock { get; set; }

        [MaxLength(255)]
        public string? UrlImage { get; set; }
    }
} 
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
    public class CategoryDTO
    {
        public int CategoryId { get; set; }

        [Required, MaxLength(40)]
        public string CategoryName { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
    }

    public class CreateCategoryDTO
    {
        [Required, MaxLength(40)]
        public string CategoryName { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
    }

    public class UpdateCategoryDTO
    {
        [Required]
        public int CategoryId { get; set; }

        [Required, MaxLength(40)]
        public string CategoryName { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
    }
} 
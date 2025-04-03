using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Entities
{
    public class Categories
    {
        [Key]
        public int CategoryId { get; set; }

        [Required, MaxLength(40)]
        public string CategoryName { get; set; }

        [MaxLength]
        public string Description { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}

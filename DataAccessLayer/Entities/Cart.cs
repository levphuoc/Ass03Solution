using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Entities
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }
        
        [Required]
        public int MemberId { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation property
        [ForeignKey("MemberId")]
        public Member Member { get; set; }
        
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}

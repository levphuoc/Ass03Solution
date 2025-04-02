using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Entities
{
    public class Cart
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CartId { get; set; }

        [Required]
        public int MemberId { get; set; }

        [ForeignKey(nameof(MemberId))]
        public Member Member { get; set; }

        // Navigation property đến CartDetail
        public ICollection<CartDetail> CartDetails { get; set; }
    }
}

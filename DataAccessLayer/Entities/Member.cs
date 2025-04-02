using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Entities
{
    public class Member
    {
        [Key]
        public int MemberId { get; set; }

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; }

        [Required, MaxLength(40)]
        public string CompanyName { get; set; }

        [Required, MaxLength(15)]
        public string City { get; set; }

        [Required, MaxLength(15)]
        public string Country { get; set; }

        [Required, MaxLength(30)]
        public string Password { get; set; }

        [Required, MaxLength(20)]
        public string Role { get; set; } = "User"; // Default role is User

        public ICollection<Order> Orders { get; set; }
        
    }
}

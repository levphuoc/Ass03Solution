using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.CustomException
{
    public class MemberNotFoundException : Exception
    {
        public MemberNotFoundException(int memberId)
            : base($"Member with ID {memberId} was not found.")
        {
            MemberId = memberId;
        }

        public MemberNotFoundException(string email)
            : base($"Member with email {email} was not found.")
        {
            Email = email;
        }

        public int? MemberId { get; }
        public string Email { get; }
    }
}

using DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.IServices
{
    public interface IMemberService
    {
        Task<IEnumerable<Member>> GetMembersAsync();
        Task<Member> GetMemberByIdAsync(int id);
        Task AddMemberAsync(Member member);
        Task UpdateMemberAsync(Member member);
        Task DeleteMemberAsync(int id);
        Task<bool> UpdateProfileAsync(int memberId, string companyName, string city, string email, string password);
        Task<IEnumerable<Member>> SearchMembersAsync(string email, string companyName);
    }
}

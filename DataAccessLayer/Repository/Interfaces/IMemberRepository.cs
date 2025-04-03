using DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository.Interfaces
{
    public interface IMemberRepository : IRepository<Member>
    {
        Task<Member?> GetByEmailAsync(string email);
        Task<IEnumerable<Member>> SearchAsync(string email, string companyName);
    }
}

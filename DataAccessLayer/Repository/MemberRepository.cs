using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using DataAccessLayer.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{
    public class MemberRepository : GenericRepository<Member>, IMemberRepository
    {
        public MemberRepository(EStoreDbContext context) : base(context) { }

        public async Task<Member?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(m => m.Email == email);
        }

        public async Task<IEnumerable<Member>> SearchAsync(string email, string companyName)
        {
            return await _context.Members
                .Where(m => (string.IsNullOrEmpty(email) || m.Email.Contains(email)) &&
                            (string.IsNullOrEmpty(companyName) || m.CompanyName.Contains(companyName)))
                .ToListAsync();
        }
    }
}

using DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.IServices
{
    public interface ICartService
    {
        Task<List<CartDetail>> GetAllCartDetailById(int userId);
        Task DeleteCartAndItemsByUserIdAsync(int MemberId);
    }
}

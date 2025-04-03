﻿using DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository.Interfaces
{
    public interface ICartRepository : IRepository<Cart>
    {
        Task<List<CartDetail>> GetCartItemsByCartIdAsync(int cartId);
        Task DeleteCartAndItemsByMemberIdAsync(int memberId);
    }
}

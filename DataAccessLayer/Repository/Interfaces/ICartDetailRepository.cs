﻿using DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository.Interfaces
{
    public interface ICartDetailRepository : IRepository<CartDetail>
    {
        Task<List<CartDetail>> GetAllCartDetailById(int userId);
    }
}

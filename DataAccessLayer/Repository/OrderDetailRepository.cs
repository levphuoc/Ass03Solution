﻿using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using DataAccessLayer.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{
    public class OrderDetailRepository : GenericRepository<OrderDetail>, IOrderDetailRepository
    {
       

        public OrderDetailRepository(EStoreDbContext context) : base(context) { }

    }
}

using DataAccessLayer.Data;
using DataAccessLayer.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{
    public class OrderDetailRepository : IOrderDetailRepository
    {
        private readonly eStoreDbContext _context;

        public OrderDetailRepository(eStoreDbContext context)
        {
            _context = context;
        }
    }
    }

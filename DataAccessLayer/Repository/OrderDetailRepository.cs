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
        private readonly EStoreDbContext _context;

        public OrderDetailRepository(EStoreDbContext context)
        {
            _context = context;
        }
    }
    }

using BLL.Hubs;
using BLL.Services.IServices;
using DataAccessLayer.Entities;
using DataAccessLayer.Repository.Interfaces;
using DataAccessLayer.UnitOfWork;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;  // Thêm namespace cho ILogger
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<MemberHub> _hubContext;
        private readonly ILogger<CartService> _logger; // Thêm ILogger vào class

        public CartService(IUnitOfWork unitOfWork, IHubContext<MemberHub> hubContext, ILogger<CartService> logger)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _logger = logger; // Khởi tạo ILogger
        }

        public async Task<List<CartDetail>> GetAllCartDetailById(int userId)
        {
           
                
                var cart = await _unitOfWork.Carts.FirstOrDefaultAsync(n => n.MemberId == userId);

               
                if (cart == null)
                {
                  
                    return new List<CartDetail>(); 
                }

              

               
                var cartDetails = await _unitOfWork.CartDetails.FindAsync(cd => cd.CartId == cart.CartId);

              
                if (cartDetails == null || !cartDetails.Any())
                {
                  
                    return new List<CartDetail>(); 
                }



            return (List<CartDetail>)cartDetails;

        }

    }
}

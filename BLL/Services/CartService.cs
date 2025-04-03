using AutoMapper.Execution;
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
      
        private readonly ILogger<CartService> _logger; 

        private readonly ICartDetailRepository _cartDetailRepository;
        private readonly ICartRepository _cartRepository;
        public CartService(ICartDetailRepository cartDetailRepository, ICartRepository cartRepository)
        {
            _cartDetailRepository = cartDetailRepository;
            _cartRepository = cartRepository;
        }

        public async Task<List<CartDetail>> GetAllCartDetailById(int userId)
        {
            return await _cartDetailRepository.GetAllCartDetailById(userId);
        }

        public async Task DeleteCartAndItemsByUserIdAsync(int MemberId)
        {

            await _cartRepository.DeleteCartAndItemsByMemberIdAsync(MemberId);
        }
        }





    
}

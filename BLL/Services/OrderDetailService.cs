using BLL.DTOs;
using BLL.Services.IServices;
using DataAccessLayer.Entities;
using DataAccessLayer.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductService _productService;

        public OrderDetailService(IUnitOfWork unitOfWork, IProductService productService)
        {
            _unitOfWork = unitOfWork;
            _productService = productService;
        }

        public async Task AddOrderDetailsAsync(IEnumerable<OrderItemDTO> orderDetails)
        {
            // First, create a dictionary of product quantities to decrease stock
            var productQuantities = new Dictionary<int, int>();
            
            foreach (var detail in orderDetails)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = detail.OrderId,
                    ProductId = detail.ProductId,
                    Quantity = detail.Quantity,
                    UnitPrice = detail.UnitPrice,
                    Discount = detail.Discount
                };

                await _unitOfWork.OrderDetails.AddAsync(orderDetail);
                
                // Add to the dictionary for stock update
                if (!productQuantities.ContainsKey(detail.ProductId))
                {
                    productQuantities[detail.ProductId] = detail.Quantity;
                }
                else
                {
                    productQuantities[detail.ProductId] += detail.Quantity;
                }
            }

            // Save the order details first
            await _unitOfWork.SaveChangesAsync();
            
            // Now update the product stock for all products in one operation
            if (productQuantities.Any())
            {
                bool stockUpdateResult = await _productService.DecreaseStockForMultipleProductsAsync(productQuantities);
                if (!stockUpdateResult)
                {
                    Console.WriteLine("Failed to update product stock for one or more products");
                }
                else
                {
                    Console.WriteLine($"Successfully decreased stock for {productQuantities.Count} products");
                }
            }
        }
        public async Task<List<OrderItemDTO>> GetOrderItemsByOrderIdAsync(int orderId)
        {
            var orderDetails = await _unitOfWork.OrderDetails
                .FindAsync(od => od.OrderId == orderId);

            var orderItems = new List<OrderItemDTO>();

            foreach (var od in orderDetails)
            {
                if (od.ProductId <= 0)
                {
                    throw new Exception("ProductId is required and must be valid.");
                }
                var product = await _unitOfWork.Products.GetByIdAsync(od.ProductId);

                orderItems.Add(new OrderItemDTO
                {
                    OrderId = od.OrderId,
                    ProductId = od.ProductId,
                    ProductName = product?.ProductName ?? "Unknown",
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice,
                    Discount = od.Discount
                });
            }

            return orderItems;
        }
        
        public async Task DeleteOrderItemsByOrderIdAsync(int orderId)
        {
            var orderItems = await _unitOfWork.OrderDetails.FindAsync(x => x.OrderId == orderId);
            if (orderItems != null)
            {
                foreach (var orderItem in orderItems)
                {
                    _unitOfWork.OrderDetails.DeleteAsync(orderItem.OrderId);
                }
                await _unitOfWork.SaveChangesAsync();
            }
        }


    }
}

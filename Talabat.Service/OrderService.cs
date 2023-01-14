using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.IRepositories;
using Talabat.Core.IServices;
using Talabat.Core.Specifications;

namespace Talabat.Service
{
    public class OrderService : IOrderService
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;

        public OrderService(IBasketRepository basketRepository,IUnitOfWork unitOfWork, IPaymentService paymentService)
        {
            _basketRepository = basketRepository;
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
        }
        public async Task<Order> CreateOrderAsync(string buyerEmail, string basketId, int deliveryMethodId, Address shippingAddress)
        {
            var basket = await _basketRepository.GetBasketAsync(basketId);

            var orderItems = new List<OrderItem>();

            foreach (var item in basket.Items)
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);
                var producItemOrdered = new ProductItemOrdered(product.Id, product.Name, product.PictureUrl);
                var orderItem = new OrderItem(producItemOrdered, product.Price, item.Quantity);

                orderItems.Add(orderItem);
            }

            var subTotal = orderItems.Sum(item => item.Price * item.Quantity);

            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(deliveryMethodId);

            var spec = new OrderByPaymentIntentIdSpecification(basket.PaymentIntentId);
            var existingOrder = await _unitOfWork.Repository<Order>().GetByIdWithSpecAsync(spec);

            if(existingOrder == null)
            {
                _unitOfWork.Repository<Order>().Delete(existingOrder);
                await _paymentService.CreateOrUpdatePaymentIntent(basketId);
            }

            var order = new Order(buyerEmail, shippingAddress, deliveryMethod, orderItems, subTotal, basket.PaymentIntentId);

            await _unitOfWork.Repository<Order>().CreateAsync(order);

            var result = await _unitOfWork.Complete();

            if (result <= 0) return null;

            return order;
        }

        public async Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync()
        {
            var deliveryMethods = await _unitOfWork.Repository<DeliveryMethod>().GetAllAsync();
            return deliveryMethods;
        }

        public async Task<Order> GetOrderByIdForUserAsync(int orderId, string buyerEmail)
        {
            var spec = new OrderWithItemsAndDeliveryMethodSpecification(orderId, buyerEmail);
            var order = await _unitOfWork.Repository<Order>().GetByIdWithSpecAsync(spec);
            return order;
        }

        public async Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail)
        {
            var spec = new OrderWithItemsAndDeliveryMethodSpecification(buyerEmail);

            var orders = await _unitOfWork.Repository<Order>().GetAllWithSpecAsync(spec);

            return orders;
        }
    }
}

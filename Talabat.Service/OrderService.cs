using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.IRepositories;
using Talabat.Core.IServices;

namespace Talabat.Service
{
    public class OrderService : IOrderService
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IGenericRepository<Product> _productsRepo;
        private readonly IGenericRepository<DeliveryMethod> _deliveryMethods;
        private readonly IGenericRepository<Order> _ordersRepo;

        public OrderService(IBasketRepository basketRepository,
            IGenericRepository<Product> productsRepo,
            IGenericRepository<DeliveryMethod> deliveryMethods,
            IGenericRepository<Order> ordersRepo)
        {
            _basketRepository = basketRepository;
            _productsRepo = productsRepo;
            _deliveryMethods = deliveryMethods;
            _ordersRepo = ordersRepo;
        }
        public async Task<Order> CreateOrderAsync(string buyerEmail, string basketId, int deliveryMethodId, Address shippingAddress)
        {
            var basket = await _basketRepository.GetBasketAsync(basketId);

            var orderItems = new List<OrderItem>();

            foreach (var item in basket.Items)
            {
                var product = await _productsRepo.GetByIdAsync(item.Id);
                var producItemOrdered = new ProductItemOrdered(product.Id, product.Name, product.PictureUrl);
                var orderItem = new OrderItem(producItemOrdered, product.Price, item.Quantity);

                orderItems.Add(orderItem);
            }

            var subTotal = orderItems.Sum(item => item.Price * item.Quantity);

            var deliveryMethod = await _deliveryMethods.GetByIdAsync(deliveryMethodId);

            var order = new Order(buyerEmail, shippingAddress, deliveryMethod, orderItems, subTotal);

            await _ordersRepo.CreateAsync(order);
            return order;
        }

        public Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Order> GetOrderByIdForUserAsync(int orderId, string buyerEmail)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail)
        {
            throw new NotImplementedException();
        }
    }
}

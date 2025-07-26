using Microsoft.Extensions.Logging;
using LegacyOrderService.Data;
using LegacyOrderService.Models;
using LegacyOrderService.Exceptions;

namespace LegacyOrderService.Services
{
    public class OrderService
    {
        private readonly ProductRepository _productRepository;
        private readonly OrderRepository _orderRepository;
        private readonly ILogger<OrderService> _logger;

        public OrderService(ProductRepository productRepository, OrderRepository orderRepository, ILogger<OrderService> logger)
        {
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task ProcessOrderAsync(string customerName, string productName, string quantityInput)
        {
            _logger.LogInformation("Processing order for customer: {CustomerName}, product: {ProductName}, quantity: {QuantityInput}",
                customerName, productName, quantityInput);

            try
            {
                if (string.IsNullOrWhiteSpace(customerName))
                    throw new ArgumentException("Customer name cannot be empty.");

                if (!_productRepository.ProductExists(productName))
                    throw new ProductNotFoundException($"Product '{productName}' not found.");

                if (!int.TryParse(quantityInput, out int quantity) || quantity <= 0)
                    throw new ArgumentException("Quantity must be a positive integer.");

                double price = await _productRepository.GetPriceAsync(productName);

                var order = new Order(customerName, productName, quantity, price);

                _logger.LogInformation("Order created:\n{@Order}", order);

                await _orderRepository.SaveAsync(order);

                _logger.LogInformation("Order complete!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the order.");
                throw;
            }
        }
    }
}

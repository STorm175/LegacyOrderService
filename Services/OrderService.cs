using LegacyOrderService.Data;
using LegacyOrderService.Models;

namespace LegacyOrderService.Services
{
    public class OrderService
    {
        private readonly ProductRepository _productRepository;
        private readonly OrderRepository _orderRepository;

        public OrderService(ProductRepository productRepository, OrderRepository orderRepository)
        {
            _productRepository = productRepository;
            _orderRepository = orderRepository;
        }

        public void ProcessOrder(string customerName, string productName, string quantityInput)
        {
            if (string.IsNullOrWhiteSpace(customerName))
                throw new ArgumentException("Customer name cannot be empty.");

            if (!_productRepository.ProductExists(productName))
                throw new ArgumentException($"Product '{productName}' not found.");

            if (!int.TryParse(quantityInput, out int quantity) || quantity <= 0)
                throw new ArgumentException("Quantity must be a positive integer.");

            double price = _productRepository.GetPrice(productName);

            var order = new Order(customerName, productName, quantity, price);

            Console.WriteLine("Order complete!");
            Console.WriteLine(order);
            Console.WriteLine("Saving order to database...");
            _orderRepository.Save(order);
            Console.WriteLine("Done.");
        }
    }
}
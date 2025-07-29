using LegacyOrderService.Exceptions;
using LegacyOrderService.Interfaces;
using Microsoft.Extensions.Logging;

namespace LegacyOrderService.Data
{
    public class ProductRepository : IProductRepository
    {
        private readonly Dictionary<string, double> _productPrices;
        private readonly ILogger<ProductRepository> _logger;

        public ProductRepository(ILogger<ProductRepository> logger)
        {
            _logger = logger;
            _productPrices = new Dictionary<string, double>
            {
                ["Widget"] = 12.99,
                ["Gadget"] = 15.49,
                ["Doohickey"] = 8.75
            };
        }

        /// <summary>
        /// Checks if a product exists in the repository.
        /// </summary>
        public bool ProductExists(string productName)
        {
            _logger.LogInformation("Checking if product exists: {ProductName}", productName);
            return _productPrices.ContainsKey(productName);
        }

        /// <summary>
        /// Gets the price of a product asynchronously.
        /// </summary>
        public async Task<double> GetPriceAsync(string productName)
        {
            _logger.LogInformation("Fetching price for product: {ProductName}", productName);

            // Simulate an asynchronous operation
            await Task.Delay(100);

            if (_productPrices.TryGetValue(productName, out var price))
            {
                _logger.LogInformation("Price for product {ProductName} is {Price}", productName, price);
                return price;
            }

            _logger.LogWarning("Product not found: {ProductName}", productName);
            throw new ProductNotFoundException($"Product '{productName}' not found.");
        }
    }
}

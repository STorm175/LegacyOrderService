using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LegacyOrderService.Data
{
    public class ProductRepository
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

        /// <summary>
        /// Adds or updates a product in the repository.
        /// </summary>
        public void AddOrUpdateProduct(string productName, double price)
        {
            if (string.IsNullOrWhiteSpace(productName))
                throw new ArgumentException("Product name cannot be empty.", nameof(productName));
            if (price < 0)
                throw new ArgumentException("Price cannot be negative.", nameof(price));

            _productPrices[productName] = price;
            _logger.LogInformation("Product added/updated: {ProductName} with price {Price}", productName, price);
        }

        /// <summary>
        /// Removes a product from the repository.
        /// </summary>
        public void RemoveProduct(string productName)
        {
            if (_productPrices.Remove(productName))
            {
                _logger.LogInformation("Product removed: {ProductName}", productName);
            }
            else
            {
                _logger.LogWarning("Attempted to remove non-existent product: {ProductName}", productName);
            }
        }
    }

    /// <summary>
    /// Custom exception for product not found scenarios.
    /// </summary>
    public class ProductNotFoundException : Exception
    {
        public ProductNotFoundException(string message) : base(message) { }
    }
}

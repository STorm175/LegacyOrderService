using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LegacyOrderService.Data;
using LegacyOrderService.Services;

namespace LegacyOrderService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Configure Dependency Injection and Logging
            var serviceProvider = new ServiceCollection()
                .AddLogging(configure =>
                {
                    configure.AddSimpleConsole(options =>
                    {
                        options.SingleLine = true; // Log everything in a single line
                        options.TimestampFormat = "yyyy-MM-dd HH:mm:ss "; // Add a timestamp
                        options.IncludeScopes = false; // Disable scopes
                    });
                    configure.SetMinimumLevel(LogLevel.Information);
                }) // Add console logging
                .AddSingleton<ProductRepository>()
                .AddSingleton<OrderRepository>()
                .AddSingleton<OrderService>()
                .BuildServiceProvider();

            // Get the logger
            var logger = serviceProvider.GetService<ILogger<Program>>();
            logger.LogInformation("Application started.");

            logger.LogInformation("Welcome to Order Processor!");
            logger.LogInformation("Enter customer name:");
            string name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                logger.LogWarning("Customer name is required.");
                return;
            }

            logger.LogInformation("Enter product name:");
            string product = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(product))
            {
                logger.LogWarning("Product name is required.");
                return;
            }
            
            logger.LogInformation("Enter quantity:");
            string quantityInput = Console.ReadLine();
            if (!int.TryParse(quantityInput, out int quantity) || quantity <= 0)
            {
                logger.LogWarning("Quantity must be a positive integer.");
                return;
            }

            var orderService = serviceProvider.GetService<OrderService>();
            try
            {
                await orderService.ProcessOrderAsync(name, product, quantityInput);
                logger.LogInformation("Order processed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while processing the order.");
            }

            logger.LogInformation("Application ended.");
        }
    }
}
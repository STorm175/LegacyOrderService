using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LegacyOrderService.Data;
using LegacyOrderService.Services;
using Microsoft.Extensions.Configuration;
using System.Data.Common;
using LegacyOrderService.Interfaces;

namespace LegacyOrderService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            DbProviderFactories.RegisterFactory(
            "Microsoft.Data.Sqlite",
            Microsoft.Data.Sqlite.SqliteFactory.Instance
                );
            var serviceProvider = ConfigureServices(configuration);
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Application started.");

            var (name, product, quantity) = GetUserInput(logger);
            if (name == null || product == null || quantity <= 0)
            {
                logger.LogWarning("Invalid input. Exiting application.");
                return;
            }

            await ProcessOrderAsync(serviceProvider, logger, name, product, quantity);

            logger.LogInformation("Application ended.");
        }

        private static ServiceProvider ConfigureServices(IConfiguration configuration)
        {
            return new ServiceCollection()
                .AddSingleton(configuration) // register IConfiguration
                .AddLogging(configure =>
                {
                    configure.AddSimpleConsole(options =>
                    {
                        options.SingleLine = true;
                        options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
                        options.IncludeScopes = false;
                    });
                    configure.SetMinimumLevel(LogLevel.Information);
                })
                .AddScoped<IProductRepository, ProductRepository>()
                .AddScoped<IOrderRepository, SqliteOrderRepository>()
                .AddScoped<OrderService>()
                .BuildServiceProvider();
        }

        private static (string? name, string? product, int quantity) GetUserInput(ILogger logger)
        {
            logger.LogInformation("Welcome to Order Processor!");

            logger.LogInformation("Enter customer name:");
            var name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                logger.LogWarning("Customer name is required.");
                return (null, null, 0);
            }

            logger.LogInformation("Enter product name:");
            var product = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(product))
            {
                logger.LogWarning("Product name is required.");
                return (null, null, 0);
            }

            logger.LogInformation("Enter quantity:");
            var quantityInput = Console.ReadLine();
            if (!int.TryParse(quantityInput, out int quantity) || quantity <= 0)
            {
                logger.LogWarning("Quantity must be a positive integer.");
                return (null, null, 0);
            }

            return (name, product, quantity);
        }

        private static async Task ProcessOrderAsync(
            IServiceProvider serviceProvider,
            ILogger logger,
            string name,
            string product,
            int quantity)
        {
            try
            {
                var orderService = serviceProvider.GetRequiredService<OrderService>();
                await orderService.ProcessOrderAsync(name, product, quantity.ToString());
                logger.LogInformation("Order processed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while processing the order.");
            }
        }
    }
}

using LegacyOrderService.Data;
using LegacyOrderService.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LegacyOrderService
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ProductRepository>()
                .AddSingleton<OrderRepository>()
                .AddSingleton<OrderService>()
                .BuildServiceProvider();

            var orderService = serviceProvider.GetService<OrderService>();

            Console.WriteLine("Welcome to Order Processor!");
            Console.WriteLine("Enter customer name:");
            string name = Console.ReadLine();

            Console.WriteLine("Enter product name:");
            string product = Console.ReadLine();
            var productRepo = new ProductRepository();

            Console.WriteLine("Enter quantity:");
            string quantityInput = Console.ReadLine();

            try
            {
                orderService.ProcessOrder(name, product, quantityInput);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}

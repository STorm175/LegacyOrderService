using System;
using LegacyOrderService.Models;
using LegacyOrderService.Data;

namespace LegacyOrderService
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Order Processor!");
            Console.WriteLine("Enter customer name:");
            string name = Console.ReadLine();

            Console.WriteLine("Enter product name:");
            string product = Console.ReadLine();
            var productRepo = new ProductRepository();
            double price = productRepo.GetPrice(product);


            Console.WriteLine("Enter quantity:");
            int qty = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Processing order...");

            Order order = new Order();
            order.CustomerName = name;
            order.ProductName = product;
            order.Quantity = qty;
            order.Price = 10.0;

            double total = order.Quantity * order.Price;

            Console.WriteLine("Order complete!");
            Console.WriteLine("Customer: " + order.CustomerName);
            Console.WriteLine("Product: " + order.ProductName);
            Console.WriteLine("Quantity: " + order.Quantity);
            Console.WriteLine("Total: $" + price);

            Console.WriteLine("Saving order to database...");
            var repo = new OrderRepository();
            repo.Save(order);
            Console.WriteLine("Done.");
        }
    }
}

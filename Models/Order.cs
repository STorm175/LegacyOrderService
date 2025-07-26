namespace LegacyOrderService.Models
{
    public record Order(string CustomerName, string ProductName, int Quantity, double Price)
    {
        public double Total => Quantity * Price;

        public override string ToString()
        {
            return $"Customer: {CustomerName}\n" +
                   $"Product: {ProductName}\n" +
                   $"Quantity: {Quantity}\n" +
                   $"Total: ${Total}";
        }
    }
}

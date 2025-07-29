namespace LegacyOrderService.Exceptions
{

    /// <summary>
    /// Custom exception for product not found scenarios.
    /// </summary>
    public class ProductNotFoundException : Exception
    {
        public ProductNotFoundException(string message) : base(message) { }
    }
}

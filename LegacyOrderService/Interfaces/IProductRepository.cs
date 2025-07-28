// IOrderRepository.cs
using LegacyOrderService.Models;

namespace LegacyOrderService.Interfaces
{
    public interface IProductRepository
    {
        bool ProductExists(string productName);
        Task<double> GetPriceAsync(string productName);
    }
}

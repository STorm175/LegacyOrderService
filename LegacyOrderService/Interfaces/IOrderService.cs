// IOrderRepository.cs
using LegacyOrderService.Models;

namespace LegacyOrderService.Interfaces
{
    public interface IOrderService
    {
        Task ProcessOrderAsync(string customerName, string productName, string quantityInput);
    }
}

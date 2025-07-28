using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using LegacyOrderService.Data;
using LegacyOrderService.Exceptions;
using System.Threading.Tasks;

namespace LegacyOrderService.Tests
{
    public class ProductRepositoryTests
    {
        private readonly Mock<ILogger<ProductRepository>> _loggerMock = new();

        private ProductRepository CreateRepo() =>
            new ProductRepository(_loggerMock.Object);

        [Fact]
        public void ProductExists_ExistingProduct_ReturnsTrue()
        {
            var repo = CreateRepo();

            bool exists = repo.ProductExists("Widget");

            Assert.True(exists);
        }

        [Fact]
        public void ProductExists_NonExistingProduct_ReturnsFalse()
        {
            var repo = CreateRepo();

            bool exists = repo.ProductExists("NonExistent");

            Assert.False(exists);
        }

        [Fact]
        public async Task GetPriceAsync_ExistingProduct_ReturnsCorrectPrice()
        {
            var repo = CreateRepo();

            double price = await repo.GetPriceAsync("Gadget");

            Assert.Equal(15.49, price, precision: 2);
        }

        [Fact]
        public async Task GetPriceAsync_NonExistingProduct_ThrowsProductNotFoundException()
        {
            var repo = CreateRepo();

            await Assert.ThrowsAsync<ProductNotFoundException>(() =>
                repo.GetPriceAsync("UnknownItem"));
        }
    }
}

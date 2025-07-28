using Moq;
using Microsoft.Extensions.Logging;
using LegacyOrderService.Services;
using LegacyOrderService.Interfaces;
using LegacyOrderService.Models;
using LegacyOrderService.Exceptions;

namespace LegacyOrderService.Tests
{
    public class OrderServiceTests
    {
        private readonly Mock<IProductRepository> _productRepoMock = new();
        private readonly Mock<IOrderRepository> _orderRepoMock = new();
        private readonly Mock<ILogger<OrderService>> _loggerMock = new();

        private OrderService CreateService() =>
            new OrderService(_productRepoMock.Object, _orderRepoMock.Object, _loggerMock.Object);

        [Fact]
        public async Task ProcessOrderAsync_ValidInput_ShouldSaveOrder()
        {
            // Arrange
            string customer = "Alice";
            string product = "Widget";
            string quantity = "3";
            double unitPrice = 10.0;

            _productRepoMock.Setup(r => r.ProductExists(product)).Returns(true);
            _productRepoMock.Setup(r => r.GetPriceAsync(product)).ReturnsAsync(unitPrice);

            var service = CreateService();

            // Act
            await service.ProcessOrderAsync(customer, product, quantity);

            // Assert
            _orderRepoMock.Verify(repo => repo.SaveAsync(
                It.Is<Order>(o =>
                    o.CustomerName == customer &&
                    o.ProductName == product &&
                    o.Quantity == 3 &&
                    o.Price == unitPrice
                )), Times.Once);
        }

        [Fact]
        public async Task ProcessOrderAsync_InvalidQuantity_ShouldThrowArgumentException()
        {
            _productRepoMock.Setup(r => r.ProductExists("Pen")).Returns(true);
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.ProcessOrderAsync("Bob", "Pen", "abc"));
        }

        [Fact]
        public async Task ProcessOrderAsync_ProductDoesNotExist_ShouldThrowProductNotFoundException()
        {
            _productRepoMock.Setup(r => r.ProductExists("GhostProduct")).Returns(false);

            var service = CreateService();

            await Assert.ThrowsAsync<ProductNotFoundException>(() =>
                service.ProcessOrderAsync("Bob", "GhostProduct", "1"));
        }

        [Fact]
        public async Task ProcessOrderAsync_EmptyCustomerName_ShouldThrowArgumentException()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.ProcessOrderAsync("", "Pen", "1"));
        }
    }
}

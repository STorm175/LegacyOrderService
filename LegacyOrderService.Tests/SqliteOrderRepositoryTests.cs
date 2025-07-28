using Moq;
using Microsoft.Extensions.Logging;
using LegacyOrderService.Data;
using LegacyOrderService.Models;
using Microsoft.Data.Sqlite;
using Polly;
using System.Data.Common;
using Polly.Retry;

namespace LegacyOrderService.Tests
{
    public class SqliteOrderRepositoryTests
    {
        [Fact]
        public async Task SaveAsync_InsertsOrder_Successfully()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SqliteOrderRepository>>();
            var connectionString = "Data Source=SharedDb;Mode=Memory;Cache=Shared";

            // 1️ Mở và giữ kết nối sống suốt test để không mất DB
            using var keepAliveConnection = new SqliteConnection(connectionString);
            keepAliveConnection.Open();

            // 2 Tạo bảng
            using var cmd = keepAliveConnection.CreateCommand();
            cmd.CommandText = @"
        CREATE TABLE Orders (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            CustomerName TEXT NOT NULL,
            ProductName TEXT NOT NULL,
            Quantity INTEGER NOT NULL,
            Price REAL NOT NULL
        );";
            cmd.ExecuteNonQuery();

            // 3️⃣ Mock DbProviderFactory trả về SqliteConnection mới (cùng connection string)
            var factoryMock = new Mock<DbProviderFactory>();
            factoryMock
                .Setup(f => f.CreateConnection())
                .Returns(() => new SqliteConnection(connectionString));

            //  Retry policy NoOp
            AsyncRetryPolicy noRetryPolicy = Policy.Handle<DbException>()
                .WaitAndRetryAsync(0, _ => TimeSpan.Zero);

            var repo = new SqliteOrderRepository(
                loggerMock.Object,
                connectionString,
                factoryMock.Object,
                noRetryPolicy);

            var order = new Order("Test User", "Test Product", 2, 50.0);

            // Act
            await repo.SaveAsync(order);

            // Assert
            using var verifyCmd = keepAliveConnection.CreateCommand();
            verifyCmd.CommandText = "SELECT COUNT(*) FROM Orders";
            var count = (long)verifyCmd.ExecuteScalar();

            Assert.Equal(1, count);
        }


        [Fact]
        public async Task SaveAsync_ThrowsException_WhenConnectionIsNull()
        {
            // Arrange
            var logger = new Mock<ILogger<SqliteOrderRepository>>();
            var factoryMock = new Mock<DbProviderFactory>();
            factoryMock.Setup(f => f.CreateConnection()).Returns((DbConnection)null); // simulate null
            AsyncRetryPolicy noRetryPolicy = Policy
                .Handle<DbException>()
                .WaitAndRetryAsync(0, _ => TimeSpan.Zero);

            var repo = new SqliteOrderRepository(logger.Object, "fake", factoryMock.Object, noRetryPolicy);

            var order = new Order("A", "B", 1, 2);

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.SaveAsync(order));
        }

        [Fact]
        public async Task SaveAsync_RetryThreeTimes_OnTransientFailure()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SqliteOrderRepository>>();
            var connectionString = "Data Source=SharedDb;Mode=Memory;Cache=Shared";

            using var conn = new SqliteConnection(connectionString);
            conn.Open();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    CREATE TABLE Orders (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        CustomerName TEXT NOT NULL,
                        ProductName TEXT NOT NULL,
                        Quantity INTEGER NOT NULL CHECK(Quantity >= 0),
                        Price REAL NOT NULL
                    );";
                cmd.ExecuteNonQuery();
            }

            int attempt = 0;

            // Custom retry policy that increments retry count
            var retryPolicy = Policy
                .Handle<DbException>()
                .RetryAsync(3, onRetry: (ex, retryCount, context) =>
                {
                    attempt++;
                });

            // Repository
            var repo = new SqliteOrderRepository(
                loggerMock.Object,
                connectionString,
                SqliteFactory.Instance,
                retryPolicy);

            var order = new Order("Retry User", "Product", -1, 100); // Quantity < 0 → lỗi

            // Act + Assert
            await Assert.ThrowsAsync<SqliteException>(() => repo.SaveAsync(order));

            // attempt will be 3, because policy retries 3 times after first failure
            Assert.Equal(3, attempt);
        }
    }
}

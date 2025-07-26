using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using LegacyOrderService.Models;

namespace LegacyOrderService.Data
{
    public class OrderRepository
    {
        private readonly string _connectionString = $"Data Source={Path.Combine(AppContext.BaseDirectory, "orders.db")}";
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(ILogger<OrderRepository> logger)
        {
            _logger = logger;
        }

        public async Task SaveAsync(Order order)
        {
            _logger.LogInformation("Saving order to the database:\n{@Order}", order);

            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();
                _logger.LogDebug("Database connection opened.");

                using var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Orders (CustomerName, ProductName, Quantity, Price)
                    VALUES (@CustomerName, @ProductName, @Quantity, @Price)";
                command.Parameters.AddWithValue("@CustomerName", order.CustomerName);
                command.Parameters.AddWithValue("@ProductName", order.ProductName);
                command.Parameters.AddWithValue("@Quantity", order.Quantity);
                command.Parameters.AddWithValue("@Price", order.Price);

                await command.ExecuteNonQueryAsync();
                _logger.LogInformation("Order saved successfully!");
            }
            catch (SqliteException ex)
            {
                _logger.LogError(ex, "Database error occurred while saving the order.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while saving the order.");
                throw;
            }
        }
    }
}

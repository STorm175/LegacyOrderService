using Polly;
using Polly.Retry;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using LegacyOrderService.Models;
using LegacyOrderService.Interfaces;

namespace LegacyOrderService.Data
{
    public class SqliteOrderRepository : IOrderRepository
    {
        private readonly string _connectionString;
        private readonly DbProviderFactory _factory;
        private readonly ILogger<SqliteOrderRepository> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;

        public SqliteOrderRepository(ILogger<SqliteOrderRepository> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("OrdersDatabase")
                ?? throw new InvalidOperationException("Connection string 'OrdersDatabase' is missing.");

            var providerName = configuration["Database:ProviderName"]
                ?? throw new InvalidOperationException("Provider name is missing.");

            _factory = DbProviderFactories.GetFactory(providerName);

            _retryPolicy = Policy
                .Handle<DbException>() // catch transient DB errors
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // 2s, 4s, 8s
                    onRetry: (exception, time, retryCount, context) =>
                    {
                        _logger.LogWarning(exception, "Retry {RetryCount} after {Delay} due to transient error.", retryCount, time);
                    });
        }

        public async Task SaveAsync(Order order)
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                _logger.LogInformation("Saving order to the database:\n{@Order}", order);

                using var connection = _factory.CreateConnection();
                if (connection == null)
                    throw new InvalidOperationException("Failed to create DB connection.");

                connection.ConnectionString = _connectionString;
                await connection.OpenAsync();

                using var transaction = connection.BeginTransaction(); // start transaction

                try
                {
                    using var command = connection.CreateCommand();
                    command.Transaction = transaction; // attach transaction
                    command.CommandText = @"
                INSERT INTO Orders (CustomerName, ProductName, Quantity, Price)
                VALUES (@CustomerName, @ProductName, @Quantity, @Price)";

                    AddParameter(command, "@CustomerName", order.CustomerName);
                    AddParameter(command, "@ProductName", order.ProductName);
                    AddParameter(command, "@Quantity", order.Quantity);
                    AddParameter(command, "@Price", order.Price);

                    await command.ExecuteNonQueryAsync();

                    await transaction.CommitAsync(); // commit transaction
                    _logger.LogInformation("Order saved and transaction committed.");
                }
                catch (DbException dbEx)
                {
                    await transaction.RollbackAsync(); // rollback on error
                    _logger.LogError(dbEx, "Database error during SaveAsync. Transaction rolled back.");
                    throw;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Unexpected error during SaveAsync. Transaction rolled back.");
                    throw;
                }
            });
        }

        private void AddParameter(DbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }
    }
}

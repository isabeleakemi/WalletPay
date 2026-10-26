using MongoDB.Driver;
using WalletPay.Domain.Entities;
using WalletPay.Infrastructure.Persistence.MongoDb;
using WalletPay.Infrastructure.Repositories;

namespace WalletPay.Infrastructure.Tests.Repositories
{
    public class MongoAccountRepositoryTests
    {
        [Fact]
        public async Task AddAsync_ThenGetById_ShouldReturnSavedAccount()
        {
            // Arrange
            var connectionString =
                Environment.GetEnvironmentVariable(
                    "WALLETPAY_MONGODB_CONNECTION")
                ?? "mongodb://localhost:27017/?replicaSet=rs0";

            var databaseName =
                $"WalletPayIntegrationTests_{Guid.NewGuid():N}";

            var client = new MongoClient(connectionString);

            var settings = new MongoDbSettings
            {
                ConnectionString = connectionString,
                DatabaseName = databaseName
            };

            try
            {
                var context = new MongoDbContext(settings);
                var sessionContext = new MongoSessionContext();

                var repository = new MongoAccountRepository(
                    context,
                    sessionContext);

                var account = new Account("João", "TEST-001");

                // Act
                await repository.AddAsync(account);

                var savedAccount =
                    await repository.GetByIdAsync(account.Id);

                // Assert
                Assert.NotNull(savedAccount);
                Assert.Equal(account.Id, savedAccount.Id);
                Assert.Equal(account.Name, savedAccount.Name);
                Assert.Equal(account.Document, savedAccount.Document);
                Assert.Equal(account.Balance, savedAccount.Balance);
            }
            finally
            {
                await client.DropDatabaseAsync(databaseName);
            }
        }
    }
}

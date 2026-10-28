using WalletPay.Domain.Entities;
using WalletPay.Infrastructure.Persistence.MongoDb;
using WalletPay.Infrastructure.Repositories;

namespace WalletPay.Infrastructure.Tests.Persistence.MongoDb
{
    public class MongoTransferUnitOfWorkTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldRollbackChanges_WhenOperationFails()
        {
            var connectionString =
                Environment.GetEnvironmentVariable(
                    "WALLETPAY_MONGODB_CONNECTION")
                ?? "mongodb://localhost:27017/?replicaSet=rs0";

            var databaseName =
                $"WalletPayIntegrationTests_{Guid.NewGuid():N}";

            var settings = new MongoDbSettings
            {
                ConnectionString = connectionString,
                DatabaseName = databaseName
            };

            MongoDbConfiguration.Configure();

            var context = new MongoDbContext(settings);
            var sessionContext = new MongoSessionContext();

            var unitOfWork = new MongoTransferUnitOfWork(
                context,
                sessionContext);

            var repository = new MongoAccountRepository(
                context,
                sessionContext);

            var account = new Account("Conta de teste", "TEST-ROLLBACK");

            try
            {
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => unitOfWork.ExecuteAsync(async cancellationToken =>
                    {
                        await repository.AddAsync(account, cancellationToken);

                        throw new InvalidOperationException(
                            "Falha simulada durante a transferência.");
                    }));

                var savedAccount = await repository.GetByIdAsync(account.Id);

                Assert.Null(savedAccount);
                Assert.Null(sessionContext.Session);
            }
            finally
            {
                await context.Client.DropDatabaseAsync(databaseName);
            }
        }
    }
}

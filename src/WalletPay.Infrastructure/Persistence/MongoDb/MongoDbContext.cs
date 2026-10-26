using MongoDB.Driver;
using WalletPay.Domain.Entities;

namespace WalletPay.Infrastructure.Persistence.MongoDb
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public IMongoClient Client { get; }

        public MongoDbContext(MongoDbSettings settings)
        {
            MongoDbConfiguration.Configure();

            Client = new MongoClient(settings.ConnectionString);
            _database = Client.GetDatabase(settings.DatabaseName);
        }

        public IMongoCollection<Account> Accounts =>
            _database.GetCollection<Account>("accounts");

        public IMongoCollection<Transaction> Transactions =>
            _database.GetCollection<Transaction>("transactions");
    }
}

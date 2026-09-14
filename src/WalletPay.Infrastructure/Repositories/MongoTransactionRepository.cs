using MongoDB.Driver;
using WalletPay.Application.Interfaces;
using WalletPay.Domain.Entities;
using WalletPay.Infrastructure.Persistence.MongoDb;

namespace WalletPay.Infrastructure.Repositories
{
    public class MongoTransactionRepository : ITransactionRepository
    {
        private readonly MongoDbContext _context;

        public MongoTransactionRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<Transaction?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .Find(transaction => transaction.Id == id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddAsync(
            Transaction transaction,
            CancellationToken cancellationToken = default)
        {
            await _context.Transactions.InsertOneAsync(
                transaction,
                cancellationToken: cancellationToken);
        }

        public async Task UpdateAsync(
            Transaction transaction,
            CancellationToken cancellationToken = default)
        {
            await _context.Transactions.ReplaceOneAsync(
                x => x.Id == transaction.Id,
                transaction,
                cancellationToken: cancellationToken);
        }
    }
}

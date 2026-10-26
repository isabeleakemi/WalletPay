using MongoDB.Driver;
using WalletPay.Application.Interfaces;
using WalletPay.Domain.Entities;
using WalletPay.Domain.Enums;
using WalletPay.Infrastructure.Persistence.MongoDb;

namespace WalletPay.Infrastructure.Repositories
{
    public class MongoTransactionRepository : ITransactionRepository
    {
        private readonly MongoDbContext _context;
        private readonly MongoSessionContext _sessionContext;

        public MongoTransactionRepository(
            MongoDbContext context,
            MongoSessionContext sessionContext)
        {
            _context = context;
            _sessionContext = sessionContext;
        }

        public async Task<Transaction?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var session = _sessionContext.Session;

            if (session is not null)
            {
                return await _context.Transactions
                    .Find(session, transaction => transaction.Id == id)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return await _context.Transactions
                .Find(transaction => transaction.Id == id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddAsync(
            Transaction transaction,
            CancellationToken cancellationToken = default)
        {
            var session = _sessionContext.Session;

            if (session is not null)
            {
                await _context.Transactions.InsertOneAsync(
                    session,
                    transaction,
                    cancellationToken: cancellationToken);
                return;
            }

            await _context.Transactions.InsertOneAsync(
                transaction,
                cancellationToken: cancellationToken);
        }

        public async Task UpdateAsync(
            Transaction transaction,
            CancellationToken cancellationToken = default)
        {
            var session = _sessionContext.Session;

            if (session is not null)
            {
                await _context.Transactions.ReplaceOneAsync(
                    session,
                    x => x.Id == transaction.Id,
                    transaction,
                    cancellationToken: cancellationToken);
                return;
            }

            await _context.Transactions.ReplaceOneAsync(
                x => x.Id == transaction.Id,
                transaction,
                cancellationToken: cancellationToken);
        }

        public async Task<bool> TryStartProcessingAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var filter = Builders<Transaction>.Filter.And(
                Builders<Transaction>.Filter.Eq(
                    transaction => transaction.Id,
                    id),
                Builders<Transaction>.Filter.Eq(
                    transaction => transaction.Status,
                    TransactionStatus.Pending));

            var update = Builders<Transaction>.Update.Set(
                transaction => transaction.Status,
                TransactionStatus.Processing);

            var session = _sessionContext.Session;
            UpdateResult result;

            if (session is not null)
            {
                result = await _context.Transactions.UpdateOneAsync(
                    session,
                    filter,
                    update,
                    cancellationToken: cancellationToken);
            }
            else
            {
                result = await _context.Transactions.UpdateOneAsync(
                    filter,
                    update,
                    cancellationToken: cancellationToken);
            }

            return result.ModifiedCount == 1;
        }
    }
}

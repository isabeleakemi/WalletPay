using MongoDB.Driver;
using WalletPay.Application.Interfaces;
using WalletPay.Domain.Entities;
using WalletPay.Infrastructure.Persistence.MongoDb;

namespace WalletPay.Infrastructure.Repositories
{
    public class MongoAccountRepository : IAccountRepository
    {
        private readonly MongoDbContext _context;

        public MongoAccountRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<Account?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Accounts
                .Find(account => account.Id == id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddAsync(
            Account account,
            CancellationToken cancellationToken = default)
        {
            await _context.Accounts.InsertOneAsync(
                account,
                cancellationToken: cancellationToken);
        }

        public async Task UpdateAsync(
            Account account,
            CancellationToken cancellationToken = default)
        {
            await _context.Accounts.ReplaceOneAsync(
                x => x.Id == account.Id,
                account,
                cancellationToken: cancellationToken);
        }
    }
}

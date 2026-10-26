using MongoDB.Driver;
using WalletPay.Application.Interfaces;
using WalletPay.Domain.Entities;
using WalletPay.Infrastructure.Persistence.MongoDb;

namespace WalletPay.Infrastructure.Repositories
{
    public class MongoAccountRepository : IAccountRepository
    {
        private readonly MongoDbContext _context;
        private readonly MongoSessionContext _sessionContext;

        public MongoAccountRepository(
            MongoDbContext context,
            MongoSessionContext sessionContext)
        {
            _context = context;
            _sessionContext = sessionContext;
        }

        public async Task<Account?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var session = _sessionContext.Session;

            if (session is not null)
            {
                return await _context.Accounts
                    .Find(session, account => account.Id == id)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return await _context.Accounts
                .Find(account => account.Id == id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddAsync(
            Account account,
            CancellationToken cancellationToken = default)
        {
            var session = _sessionContext.Session;

            if (session is not null)
            {
                await _context.Accounts.InsertOneAsync(
                    session,
                    account,
                    cancellationToken: cancellationToken);
                return;
            }

            await _context.Accounts.InsertOneAsync(
                account,
                cancellationToken: cancellationToken);
        }

        public async Task UpdateAsync(
            Account account,
            CancellationToken cancellationToken = default)
        {
            var session = _sessionContext.Session;

            if (session is not null)
            {
                await _context.Accounts.ReplaceOneAsync(
                    session,
                    x => x.Id == account.Id,
                    account,
                    cancellationToken: cancellationToken);
                return;
            }

            await _context.Accounts.ReplaceOneAsync(
                x => x.Id == account.Id,
                account,
                cancellationToken: cancellationToken);
        }
    }
}

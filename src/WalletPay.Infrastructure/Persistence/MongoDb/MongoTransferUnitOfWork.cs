using WalletPay.Application.Interfaces;

namespace WalletPay.Infrastructure.Persistence.MongoDb
{
    public class MongoTransferUnitOfWork : ITransferUnitOfWork
    {
        private readonly MongoDbContext _context;
        private readonly MongoSessionContext _sessionContext;

        public MongoTransferUnitOfWork(
            MongoDbContext context,
            MongoSessionContext sessionContext)
        {
            _context = context;
            _sessionContext = sessionContext;
        }

        public async Task ExecuteAsync(
            Func<CancellationToken, Task> operation,
            CancellationToken cancellationToken = default)
        {
            using var session = await _context.Client.StartSessionAsync(
                cancellationToken: cancellationToken);

            if (_sessionContext.Session is not null)
            {
                throw new InvalidOperationException(
                    "A MongoDB session is already active.");
            }

            _sessionContext.Session = session;

            try
            {
                await session.WithTransactionAsync(
                    async (_, transactionCancellationToken) =>
                    {
                        await operation(transactionCancellationToken);
                        return true;
                    },
                    cancellationToken: cancellationToken);
            }
            finally
            {
                _sessionContext.Session = null;
            }
        }
    }
}

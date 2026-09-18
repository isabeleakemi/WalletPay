using WalletPay.Application.DTOs;
using WalletPay.Application.Interfaces;

namespace WalletPay.Application.UseCases.Transactions
{
    public class GetTransferUseCase
    {
        private readonly ITransactionRepository _transactionRepository;

        public GetTransferUseCase(
            ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<TransactionResponse?> ExecuteAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var transaction = await _transactionRepository.GetByIdAsync(
                id,
                cancellationToken);

            if (transaction is null)
                return null;

            return new TransactionResponse(
                transaction.Id,
                transaction.SourceAccountId,
                transaction.DestinationAccountId,
                transaction.Amount,
                transaction.Status,
                transaction.CreatedAt,
                transaction.ProcessedAt);
        }
    }
}

using WalletPay.Application.DTOs;
using WalletPay.Application.Interfaces;
using WalletPay.Application.Messaging;
using WalletPay.Domain.Entities;

namespace WalletPay.Application.UseCases.Transactions
{
    public class CreateTransferUseCase
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMessagePublisher _messagePublisher;

        public CreateTransferUseCase(
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            IMessagePublisher messagePublisher)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _messagePublisher = messagePublisher;
        }

        public async Task<TransactionResponse> ExecuteAsync(
            CreateTransferRequest request,
            CancellationToken cancellationToken = default)
        {
            var sourceAccount = await _accountRepository.GetByIdAsync(
                request.SourceAccountId,
                cancellationToken);

            if (sourceAccount is null)
                throw new InvalidOperationException(
                    "Source account was not found.");

            var destinationAccount = await _accountRepository.GetByIdAsync(
                request.DestinationAccountId,
                cancellationToken);

            if (destinationAccount is null)
                throw new InvalidOperationException(
                    "Destination account was not found.");

            if (sourceAccount.Balance < request.Amount)
                throw new InvalidOperationException(
                    "Insufficient balance.");

            var transaction = new Transaction(
                request.SourceAccountId,
                request.DestinationAccountId,
                request.Amount);

            await _transactionRepository.AddAsync(
                transaction,
                cancellationToken);

            var message = new TransferCreatedMessage(
            transaction.Id,
            transaction.SourceAccountId,
            transaction.DestinationAccountId,
            transaction.Amount);

            await _messagePublisher.PublishAsync(
                message,
                "transfer-created",
                cancellationToken);

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

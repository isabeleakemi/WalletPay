using WalletPay.Application.Interfaces;
using WalletPay.Application.Messaging;
using WalletPay.Domain.Enums;
using WalletPay.Domain.Exceptions;

namespace WalletPay.Application.UseCases.Transactions
{
    public class ProcessTransferUseCase
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IIdempotencyService _idempotencyService;

        public ProcessTransferUseCase(
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            IIdempotencyService idempotencyService)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _idempotencyService = idempotencyService;
        }

        public async Task ExecuteAsync(
            TransferCreatedMessage message,
            CancellationToken cancellationToken = default)
        {
            var idempotencyKey =
                $"transfer:{message.TransactionId}";

            var alreadyProcessed =
                await _idempotencyService.HasBeenProcessedAsync(
                    idempotencyKey,
                    cancellationToken);

            if (alreadyProcessed)
                return;

            var transaction = await _transactionRepository.GetByIdAsync(
                message.TransactionId,
                cancellationToken);

            if (transaction is null)
                return;

            // Evita processar novamente uma transação já finalizada.
            if (transaction.Status != TransactionStatus.Pending)
                return;

            var sourceAccount = await _accountRepository.GetByIdAsync(
                transaction.SourceAccountId,
                cancellationToken);

            var destinationAccount = await _accountRepository.GetByIdAsync(
                transaction.DestinationAccountId,
                cancellationToken);

            if (sourceAccount is null || destinationAccount is null)
            {
                transaction.StartProcessing();
                transaction.Fail();

                await _transactionRepository.UpdateAsync(
                    transaction,
                    cancellationToken);

                return;
            }

            transaction.StartProcessing();

            await _transactionRepository.UpdateAsync(
                transaction,
                cancellationToken);

            try
            {
                sourceAccount.Debit(transaction.Amount);
                destinationAccount.Credit(transaction.Amount);

                await _accountRepository.UpdateAsync(
                    sourceAccount,
                    cancellationToken);

                await _accountRepository.UpdateAsync(
                    destinationAccount,
                    cancellationToken);

                transaction.Complete();

                await _transactionRepository.UpdateAsync(
                    transaction,
                    cancellationToken);

                await _idempotencyService.MarkAsProcessedAsync(
                    idempotencyKey,
                    TimeSpan.FromHours(24),
                    cancellationToken);
            }
            catch (DomainException)
            {
                transaction.Fail();

                await _transactionRepository.UpdateAsync(
                    transaction,
                    cancellationToken);
            }
        }
    }
}

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
        private readonly ITransferUnitOfWork _unitOfWork;

        public ProcessTransferUseCase(
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            IIdempotencyService idempotencyService,
            ITransferUnitOfWork unitOfWork)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _idempotencyService = idempotencyService;
            _unitOfWork = unitOfWork;
        }

        public async Task ExecuteAsync(
            TransferCreatedMessage message,
            CancellationToken cancellationToken = default)
        {
            var idempotencyKey = $"transfer:{message.TransactionId}";

            var alreadyProcessed =
                await _idempotencyService.HasBeenProcessedAsync(
                    idempotencyKey,
                    cancellationToken);

            if (alreadyProcessed)
                return;

            await _unitOfWork.ExecuteAsync(
                async transactionCancellationToken =>
                {
                    var transaction =
                        await _transactionRepository.GetByIdAsync(
                            message.TransactionId,
                            transactionCancellationToken);

                    if (transaction is null ||
                        transaction.Status != TransactionStatus.Pending)
                    {
                        return;
                    }

                    var started =
                        await _transactionRepository.TryStartProcessingAsync(
                            transaction.Id,
                            transactionCancellationToken);

                    if (!started)
                        return;

                    transaction.StartProcessing();

                    var sourceAccount =
                        await _accountRepository.GetByIdAsync(
                            transaction.SourceAccountId,
                            transactionCancellationToken);

                    var destinationAccount =
                        await _accountRepository.GetByIdAsync(
                            transaction.DestinationAccountId,
                            transactionCancellationToken);

                    if (sourceAccount is null ||
                        destinationAccount is null)
                    {
                        transaction.Fail();

                        await _transactionRepository.UpdateAsync(
                            transaction,
                            transactionCancellationToken);

                        return;
                    }

                    try
                    {
                        sourceAccount.Debit(transaction.Amount);
                        destinationAccount.Credit(transaction.Amount);
                    }
                    catch (DomainException)
                    {
                        transaction.Fail();

                        await _transactionRepository.UpdateAsync(
                            transaction,
                            transactionCancellationToken);

                        return;
                    }

                    await _accountRepository.UpdateAsync(
                        sourceAccount,
                        transactionCancellationToken);

                    await _accountRepository.UpdateAsync(
                        destinationAccount,
                        transactionCancellationToken);

                    transaction.Complete();

                    await _transactionRepository.UpdateAsync(
                        transaction,
                        transactionCancellationToken);
                },
                cancellationToken);

            // Só marca no Redis depois que a transação MongoDB terminou.
            // A leitura também permite confirmar o resultado persistido.
            var finalTransaction =
                await _transactionRepository.GetByIdAsync(
                    message.TransactionId,
                    cancellationToken);

            if (finalTransaction?.Status == TransactionStatus.Completed)
            {
                await _idempotencyService.MarkAsProcessedAsync(
                    idempotencyKey,
                    TimeSpan.FromHours(24),
                    cancellationToken);
            }
        }
    }
}

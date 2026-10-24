using Moq;
using WalletPay.Application.Interfaces;
using WalletPay.Application.Messaging;
using WalletPay.Application.UseCases.Transactions;
using WalletPay.Domain.Entities;
using WalletPay.Domain.Enums;

namespace WalletPay.Application.Tests.UseCases.Transactions
{
    public class ProcessTransferUseCaseTests
    {
        private readonly Mock<IAccountRepository> _accountRepository = new();
        private readonly Mock<ITransactionRepository> _transactionRepository = new();
        private readonly Mock<IIdempotencyService> _idempotencyService = new();

        private ProcessTransferUseCase CreateUseCase()
        {
            return new ProcessTransferUseCase(
                _accountRepository.Object,
                _transactionRepository.Object,
                _idempotencyService.Object);
        }

        private static TransferCreatedMessage CreateMessage(Guid transactionId)
        {
            return new TransferCreatedMessage(
                TransactionId: transactionId,
                SourceAccountId: Guid.NewGuid(),
                DestinationAccountId: Guid.NewGuid(),
                Amount: 100m);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldCompleteTransfer_WhenDataIsValid()
        {
            // Arrange
            var source = new Account("João", "111");
            var destination = new Account("Maria", "222");

            source.Credit(200m);

            var transaction = new Transaction(
                source.Id,
                destination.Id,
                100m);

            var useCase = CreateUseCase();

            ConfigurePendingTransfer(transaction, source, destination);

            // Act
            await useCase.ExecuteAsync(CreateMessage(transaction.Id));

            // Assert
            Assert.Equal(100m, source.Balance);
            Assert.Equal(100m, destination.Balance);
            Assert.Equal(TransactionStatus.Completed, transaction.Status);
            Assert.NotNull(transaction.ProcessedAt);

            _accountRepository.Verify(
                x => x.UpdateAsync(
                    source,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _accountRepository.Verify(
                x => x.UpdateAsync(
                    destination,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _transactionRepository.Verify(
                x => x.TryStartProcessingAsync(
                    transaction.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _idempotencyService.Verify(
                x => x.MarkAsProcessedAsync(
                    $"transfer:{transaction.Id}",
                    TimeSpan.FromHours(24),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldFailTransfer_WhenBalanceIsInsufficient()
        {
            // Arrange
            var source = new Account("João", "111");
            var destination = new Account("Maria", "222");

            source.Credit(50m);

            var transaction = new Transaction(
                source.Id,
                destination.Id,
                100m);

            var useCase = CreateUseCase();

            ConfigurePendingTransfer(transaction, source, destination);

            // Act
            await useCase.ExecuteAsync(CreateMessage(transaction.Id));

            // Assert
            Assert.Equal(50m, source.Balance);
            Assert.Equal(0m, destination.Balance);
            Assert.Equal(TransactionStatus.Failed, transaction.Status);

            _idempotencyService.Verify(
                x => x.MarkAsProcessedAsync(
                    It.IsAny<string>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldNotProcess_WhenAlreadyProcessed()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var message = CreateMessage(transactionId);
            var useCase = CreateUseCase();

            _idempotencyService
                .Setup(x => x.HasBeenProcessedAsync(
                    $"transfer:{transactionId}",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            await useCase.ExecuteAsync(message);

            // Assert
            _transactionRepository.Verify(
                x => x.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _transactionRepository.Verify(
                x => x.TryStartProcessingAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturn_WhenTransactionDoesNotExist()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var useCase = CreateUseCase();

            _idempotencyService
                .Setup(x => x.HasBeenProcessedAsync(
                    $"transfer:{transactionId}",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _transactionRepository
                .Setup(x => x.GetByIdAsync(
                    transactionId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Transaction?)null);

            // Act
            await useCase.ExecuteAsync(CreateMessage(transactionId));

            // Assert
            _transactionRepository.Verify(
                x => x.TryStartProcessingAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _accountRepository.Verify(
                x => x.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturn_WhenTransactionIsAlreadyCompleted()
        {
            // Arrange
            var source = new Account("João", "111");
            var destination = new Account("Maria", "222");

            var transaction = new Transaction(
                source.Id,
                destination.Id,
                100m);

            transaction.StartProcessing();
            transaction.Complete();

            var useCase = CreateUseCase();

            _idempotencyService
                .Setup(x => x.HasBeenProcessedAsync(
                    $"transfer:{transaction.Id}",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _transactionRepository
                .Setup(x => x.GetByIdAsync(
                    transaction.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(transaction);

            // Act
            await useCase.ExecuteAsync(CreateMessage(transaction.Id));

            // Assert
            _transactionRepository.Verify(
                x => x.TryStartProcessingAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _accountRepository.Verify(
                x => x.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldNotContinue_WhenAtomicReservationFails()
        {
            // Arrange
            var source = new Account("João", "111");
            var destination = new Account("Maria", "222");

            source.Credit(200m);

            var transaction = new Transaction(
                source.Id,
                destination.Id,
                100m);

            var useCase = CreateUseCase();

            _idempotencyService
                .Setup(x => x.HasBeenProcessedAsync(
                    $"transfer:{transaction.Id}",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _transactionRepository
                .Setup(x => x.GetByIdAsync(
                    transaction.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(transaction);

            _transactionRepository
                .Setup(x => x.TryStartProcessingAsync(
                    transaction.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            await useCase.ExecuteAsync(CreateMessage(transaction.Id));

            // Assert
            Assert.Equal(200m, source.Balance);
            Assert.Equal(0m, destination.Balance);
            Assert.Equal(TransactionStatus.Pending, transaction.Status);

            _accountRepository.Verify(
                x => x.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _accountRepository.Verify(
                x => x.UpdateAsync(
                    It.IsAny<Account>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldFailTransfer_WhenSourceAccountDoesNotExist()
        {
            // Arrange
            var sourceId = Guid.NewGuid();
            var destination = new Account("Maria", "222");

            var transaction = new Transaction(
                sourceId,
                destination.Id,
                100m);

            var useCase = CreateUseCase();

            ConfigurePendingTransfer(
                transaction,
                null,
                destination);

            // Act
            await useCase.ExecuteAsync(CreateMessage(transaction.Id));

            // Assert
            Assert.Equal(TransactionStatus.Failed, transaction.Status);

            _accountRepository.Verify(
                x => x.UpdateAsync(
                    It.IsAny<Account>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        private void ConfigurePendingTransfer(
            Transaction transaction,
            Account? source,
            Account? destination)
        {
            _idempotencyService
                .Setup(x => x.HasBeenProcessedAsync(
                    $"transfer:{transaction.Id}",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _transactionRepository
                .Setup(x => x.GetByIdAsync(
                    transaction.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(transaction);

            _transactionRepository
                .Setup(x => x.TryStartProcessingAsync(
                    transaction.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _accountRepository
                .Setup(x => x.GetByIdAsync(
                    transaction.SourceAccountId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(source);

            _accountRepository
                .Setup(x => x.GetByIdAsync(
                    transaction.DestinationAccountId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(destination);
        }
    }
}

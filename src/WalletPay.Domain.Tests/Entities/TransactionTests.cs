using WalletPay.Domain.Entities;
using WalletPay.Domain.Enums;
using WalletPay.Domain.Exceptions;

namespace WalletPay.Domain.Tests.Entities
{
    public class TransactionTests
    {
        private static Transaction CreateTransaction(
            decimal amount = 100m)
        {
            return new Transaction(
                Guid.NewGuid(),
                Guid.NewGuid(),
                amount);
        }

        [Fact]
        public void Constructor_ShouldInitializeTransactionCorrectly()
        {
            // Arrange
            var sourceAccountId = Guid.NewGuid();
            var destinationAccountId = Guid.NewGuid();
            var amount = 100m;

            // Act
            var transaction = new Transaction(
                sourceAccountId,
                destinationAccountId,
                amount);

            // Assert
            Assert.NotEqual(Guid.Empty, transaction.Id);
            Assert.Equal(sourceAccountId, transaction.SourceAccountId);
            Assert.Equal(destinationAccountId, transaction.DestinationAccountId);
            Assert.Equal(amount, transaction.Amount);
            Assert.Equal(TransactionStatus.Pending, transaction.Status);
            Assert.NotEqual(default, transaction.CreatedAt);
            Assert.Null(transaction.ProcessedAt);
        }

        [Fact]
        public void Constructor_ShouldThrowDomainException_WhenSourceAccountIsEmpty()
        {
            // Arrange
            var destinationAccountId = Guid.NewGuid();

            // Act & Assert
            Assert.Throws<DomainException>(() =>
                new Transaction(
                    Guid.Empty,
                    destinationAccountId,
                    100m));
        }

        [Fact]
        public void Constructor_ShouldThrowDomainException_WhenDestinationAccountIsEmpty()
        {
            // Arrange
            var sourceAccountId = Guid.NewGuid();

            // Act & Assert
            Assert.Throws<DomainException>(() =>
                new Transaction(
                    sourceAccountId,
                    Guid.Empty,
                    100m));
        }

        [Fact]
        public void Constructor_ShouldThrowDomainException_WhenAccountsAreEqual()
        {
            // Arrange
            var accountId = Guid.NewGuid();

            // Act & Assert
            Assert.Throws<DomainException>(() =>
                new Transaction(
                    accountId,
                    accountId,
                    100m));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Constructor_ShouldThrowDomainException_WhenAmountIsInvalid(
            decimal amount)
        {
            // Act & Assert
            Assert.Throws<DomainException>(() =>
                CreateTransaction(amount));
        }

        [Fact]
        public void StartProcessing_ShouldChangeStatusToProcessing()
        {
            // Arrange
            var transaction = CreateTransaction();

            // Act
            transaction.StartProcessing();

            // Assert
            Assert.Equal(
                TransactionStatus.Processing,
                transaction.Status);
        }

        [Fact]
        public void Complete_ShouldChangeStatusToCompleted()
        {
            // Arrange
            var transaction = CreateTransaction();
            transaction.StartProcessing();

            // Act
            transaction.Complete();

            // Assert
            Assert.Equal(
                TransactionStatus.Completed,
                transaction.Status);

            Assert.NotNull(transaction.ProcessedAt);
        }

        [Fact]
        public void Fail_ShouldChangeStatusToFailed()
        {
            // Arrange
            var transaction = CreateTransaction();
            transaction.StartProcessing();

            // Act
            transaction.Fail();

            // Assert
            Assert.Equal(
                TransactionStatus.Failed,
                transaction.Status);

            Assert.NotNull(transaction.ProcessedAt);
        }

        [Fact]
        public void StartProcessing_ShouldThrowDomainException_WhenTransactionIsNotPending()
        {
            // Arrange
            var transaction = CreateTransaction();
            transaction.StartProcessing();

            // Act & Assert
            Assert.Throws<DomainException>(
                () => transaction.StartProcessing());
        }

        [Fact]
        public void Complete_ShouldThrowDomainException_WhenTransactionIsPending()
        {
            // Arrange
            var transaction = CreateTransaction();

            // Act & Assert
            Assert.Throws<DomainException>(
                () => transaction.Complete());
        }

        [Fact]
        public void Fail_ShouldThrowDomainException_WhenTransactionIsPending()
        {
            // Arrange
            var transaction = CreateTransaction();

            // Act & Assert
            Assert.Throws<DomainException>(
                () => transaction.Fail());
        }

        [Fact]
        public void Complete_ShouldThrowDomainException_WhenTransactionIsAlreadyCompleted()
        {
            // Arrange
            var transaction = CreateTransaction();
            transaction.StartProcessing();
            transaction.Complete();

            // Act & Assert
            Assert.Throws<DomainException>(
                () => transaction.Complete());
        }

        [Fact]
        public void Fail_ShouldThrowDomainException_WhenTransactionIsAlreadyCompleted()
        {
            // Arrange
            var transaction = CreateTransaction();
            transaction.StartProcessing();
            transaction.Complete();

            // Act & Assert
            Assert.Throws<DomainException>(
                () => transaction.Fail());
        }
    }
}

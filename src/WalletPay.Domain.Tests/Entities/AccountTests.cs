using WalletPay.Domain.Entities;
using WalletPay.Domain.Exceptions;

namespace WalletPay.Domain.Tests.Entities
{
    public class AccountTests
    {
        [Fact]
        public void Constructor_ShouldInitializeAccountWithZeroBalance()
        {
            // Arrange
            var name = "João Silva";
            var document = "12345678900";

            // Act
            var account = new Account(name, document);

            // Assert
            Assert.NotEqual(Guid.Empty, account.Id);
            Assert.Equal(name, account.Name);
            Assert.Equal(document, account.Document);
            Assert.Equal(0m, account.Balance);
            Assert.NotEqual(default, account.CreatedAt);
        }

        [Fact]
        public void Credit_ShouldIncreaseBalance_WhenAmountIsValid()
        {
            // Arrange
            var account = new Account("João Silva", "12345678900");

            // Act
            account.Credit(100m);

            // Assert
            Assert.Equal(100m, account.Balance);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Credit_ShouldThrowDomainException_WhenAmountIsInvalid(
            decimal amount)
        {
            // Arrange
            var account = new Account("João Silva", "12345678900");

            // Act
            var exception = Assert.Throws<DomainException>(
                () => account.Credit(amount));

            // Assert
            Assert.Equal(
                "Credit amount must be greater than zero.",
                exception.Message);

            Assert.Equal(0m, account.Balance);
        }

        [Fact]
        public void Debit_ShouldDecreaseBalance_WhenAmountIsValid()
        {
            // Arrange
            var account = new Account("João Silva", "12345678900");
            account.Credit(100m);

            // Act
            account.Debit(40m);

            // Assert
            Assert.Equal(60m, account.Balance);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Debit_ShouldThrowDomainException_WhenAmountIsInvalid(
            decimal amount)
        {
            // Arrange
            var account = new Account("João Silva", "12345678900");
            account.Credit(100m);

            // Act
            var exception = Assert.Throws<DomainException>(
                () => account.Debit(amount));

            // Assert
            Assert.Equal(
                "Debit amount must be greater than zero.",
                exception.Message);

            Assert.Equal(100m, account.Balance);
        }

        [Fact]
        public void Debit_ShouldThrowDomainException_WhenBalanceIsInsufficient()
        {
            // Arrange
            var account = new Account("João Silva", "12345678900");
            account.Credit(50m);

            // Act
            var exception = Assert.Throws<DomainException>(
                () => account.Debit(100m));

            // Assert
            Assert.Equal("Insufficient balance.", exception.Message);
            Assert.Equal(50m, account.Balance);
        }

        [Fact]
        public void Debit_ShouldAllowWithdrawalOfExactBalance()
        {
            // Arrange
            var account = new Account("João Silva", "12345678900");
            account.Credit(100m);

            // Act
            account.Debit(100m);

            // Assert
            Assert.Equal(0m, account.Balance);
        }
    }
}

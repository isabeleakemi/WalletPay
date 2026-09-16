using WalletPay.Domain.Exceptions;

namespace WalletPay.Domain.Entities
{
    public class Account
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Document { get; private set; } = string.Empty;
        public decimal Balance { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Account()
        {
        }

        public Account(string name, string document)
        {
            Id = Guid.NewGuid();
            Name = name;
            Document = document;
            Balance = 0;
            CreatedAt = DateTime.UtcNow;
        }

        public void Credit(decimal amount)
        {
            if (amount <= 0)
                throw new DomainException(
                    "Credit amount must be greater than zero.");

            Balance += amount;
        }

        public void Debit(decimal amount)
        {
            if (amount <= 0)
                throw new DomainException(
                    "Debit amount must be greater than zero.");

            if (Balance < amount)
                throw new DomainException(
                    "Insufficient balance.");

            Balance -= amount;
        }
    }
}

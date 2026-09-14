using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPay.Domain.Enums;

namespace WalletPay.Domain.Entities
{
    public class Transaction
    {
        public Guid Id { get; private set; }

        public Guid SourceAccountId { get; private set; }

        public Guid DestinationAccountId { get; private set; }

        public decimal Amount { get; private set; }

        public TransactionStatus Status { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime? ProcessedAt { get; private set; }

        private Transaction()
        {
        }

        public Transaction(
            Guid sourceAccountId,
            Guid destinationAccountId,
            decimal amount)
        {
            Id = Guid.NewGuid();
            SourceAccountId = sourceAccountId;
            DestinationAccountId = destinationAccountId;
            Amount = amount;
            Status = TransactionStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPay.Domain.Enums;
using WalletPay.Domain.Exceptions;

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
            if (sourceAccountId == Guid.Empty)
                throw new DomainException(
                    "Source account is required.");

            if (destinationAccountId == Guid.Empty)
                throw new DomainException(
                    "Destination account is required.");

            if (sourceAccountId == destinationAccountId)
                throw new DomainException(
                    "Source and destination accounts must be different.");

            if (amount <= 0)
                throw new DomainException(
                    "Transfer amount must be greater than zero.");

            Id = Guid.NewGuid();
            SourceAccountId = sourceAccountId;
            DestinationAccountId = destinationAccountId;
            Amount = amount;
            Status = TransactionStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        public void StartProcessing()
        {
            if (Status != TransactionStatus.Pending)
                throw new DomainException(
                    "Transaction must be pending to start processing.");

            Status = TransactionStatus.Processing;
        }

        public void Complete()
        {
            if (Status != TransactionStatus.Processing)
                throw new DomainException(
                    "Transaction must be processing to complete.");

            Status = TransactionStatus.Completed;
            ProcessedAt = DateTime.UtcNow;
        }

        public void Fail()
        {
            if (Status != TransactionStatus.Processing)
                throw new DomainException(
                    "Transaction must be processing to fail.");

            Status = TransactionStatus.Failed;
            ProcessedAt = DateTime.UtcNow;
        }
    }
}

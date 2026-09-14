using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPay.Domain.Enums;

namespace WalletPay.Application.DTOs
{
    public record TransactionResponse(
    Guid Id,
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    TransactionStatus Status,
    DateTime CreatedAt,
    DateTime? ProcessedAt);
}

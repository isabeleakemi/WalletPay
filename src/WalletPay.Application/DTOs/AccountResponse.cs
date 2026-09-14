using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletPay.Application.DTOs
{
    public record AccountResponse(
    Guid Id,
    string Name,
    string Document,
    decimal Balance,
    DateTime CreatedAt);
}

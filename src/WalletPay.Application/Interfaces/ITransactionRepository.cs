using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPay.Domain.Entities;

namespace WalletPay.Application.Interfaces
{
    public interface ITransactionRepository
    {
        Task<Transaction?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

        Task AddAsync(
            Transaction transaction,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(
            Transaction transaction,
            CancellationToken cancellationToken = default);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPay.Application.DTOs;
using WalletPay.Application.Interfaces;

namespace WalletPay.Application.UseCases.Accounts
{
    public class GetAccountUseCase
    {
        private readonly IAccountRepository _accountRepository;

        public GetAccountUseCase(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<AccountResponse?> ExecuteAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var account = await _accountRepository.GetByIdAsync(
                id,
                cancellationToken);

            if (account is null)
                return null;

            return new AccountResponse(
                account.Id,
                account.Name,
                account.Document,
                account.Balance,
                account.CreatedAt);
        }
    }
}

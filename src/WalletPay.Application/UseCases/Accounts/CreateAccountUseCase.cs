using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPay.Application.DTOs;
using WalletPay.Application.Interfaces;
using WalletPay.Domain.Entities;

namespace WalletPay.Application.UseCases.Accounts
{
    public class CreateAccountUseCase
    {
        private readonly IAccountRepository _accountRepository;

        public CreateAccountUseCase(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<AccountResponse> ExecuteAsync(
            CreateAccountRequest request,
            CancellationToken cancellationToken = default)
        {
            var account = new Account(
                request.Name,
                request.Document);

            await _accountRepository.AddAsync(
                account,
                cancellationToken);

            return new AccountResponse(
                account.Id,
                account.Name,
                account.Document,
                account.Balance,
                account.CreatedAt);
        }
    }
}

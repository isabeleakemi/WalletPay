using WalletPay.Application.DTOs;
using WalletPay.Application.Interfaces;

namespace WalletPay.Application.UseCases.Accounts
{
    public class DepositUseCase
    {
        private readonly IAccountRepository _accountRepository;

        public DepositUseCase(
            IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<AccountResponse?> ExecuteAsync(
            Guid accountId,
            DepositRequest request,
            CancellationToken cancellationToken = default)
        {
            var account = await _accountRepository.GetByIdAsync(
                accountId,
                cancellationToken);

            if (account is null)
                return null;

            account.Credit(request.Amount);

            await _accountRepository.UpdateAsync(
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

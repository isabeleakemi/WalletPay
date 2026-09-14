using Microsoft.AspNetCore.Mvc;
using WalletPay.Application.DTOs;
using WalletPay.Application.UseCases.Accounts;

namespace WalletPay.Api.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    public class AccountsController : ControllerBase
    {
        private readonly CreateAccountUseCase _createAccountUseCase;
        private readonly GetAccountUseCase _getAccountUseCase;

        public AccountsController(
            CreateAccountUseCase createAccountUseCase,
            GetAccountUseCase getAccountUseCase)
        {
            _createAccountUseCase = createAccountUseCase;
            _getAccountUseCase = getAccountUseCase;
        }

        [HttpPost]
        public async Task<ActionResult<AccountResponse>> Create(
            CreateAccountRequest request,
            CancellationToken cancellationToken)
        {
            var account = await _createAccountUseCase.ExecuteAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = account.Id },
                account);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<AccountResponse>> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var account = await _getAccountUseCase.ExecuteAsync(
                id,
                cancellationToken);

            if (account is null)
                return NotFound();

            return Ok(account);
        }
    }
}

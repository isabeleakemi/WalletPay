using Microsoft.AspNetCore.Mvc;
using WalletPay.Application.DTOs;
using WalletPay.Application.UseCases.Transactions;

namespace WalletPay.Api.Controllers
{
    [ApiController]
    [Route("api/transfers")]
    public class TransfersController : ControllerBase
    {
        private readonly CreateTransferUseCase _createTransferUseCase;

        public TransfersController(
            CreateTransferUseCase createTransferUseCase)
        {
            _createTransferUseCase = createTransferUseCase;
        }

        [HttpPost]
        public async Task<ActionResult<TransactionResponse>> Create(
            CreateTransferRequest request,
            CancellationToken cancellationToken)
        {
            var transaction = await _createTransferUseCase.ExecuteAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = transaction.Id },
                transaction);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TransactionResponse>> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            // Vamos implementar no próximo passo.
            return Ok();
        }
    }
}

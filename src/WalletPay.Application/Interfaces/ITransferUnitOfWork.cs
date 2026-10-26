namespace WalletPay.Application.Interfaces
{
    public interface ITransferUnitOfWork
    {
        Task ExecuteAsync(
            Func<CancellationToken, Task> operation,
            CancellationToken cancellationToken = default);
    }
}

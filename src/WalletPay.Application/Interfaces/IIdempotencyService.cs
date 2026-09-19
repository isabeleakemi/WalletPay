namespace WalletPay.Application.Interfaces
{
    public interface IIdempotencyService
    {
        Task<bool> HasBeenProcessedAsync(
        string key,
        CancellationToken cancellationToken = default);

        Task MarkAsProcessedAsync(
            string key,
            TimeSpan expiration,
            CancellationToken cancellationToken = default);
    }
}

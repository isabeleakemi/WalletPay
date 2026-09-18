namespace WalletPay.Application.Interfaces
{
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(
        T message,
        string queue,
        CancellationToken cancellationToken = default);
    }
}

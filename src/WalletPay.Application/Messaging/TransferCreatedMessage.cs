namespace WalletPay.Application.Messaging
{
    public record TransferCreatedMessage(
    Guid TransactionId,
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount);
}

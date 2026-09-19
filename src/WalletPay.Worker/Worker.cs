using WalletPay.Application.UseCases.Transactions;
using WalletPay.Infrastructure.Messaging.RabbitMQ;

namespace WalletPay.Worker
{
    public class Worker : BackgroundService
    {
        private readonly RabbitMqTransferConsumer _consumer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<Worker> _logger;

        public Worker(
            RabbitMqTransferConsumer consumer,
            IServiceScopeFactory scopeFactory, 
            ILogger<Worker> logger)
        {
            _consumer = consumer;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
            "WalletPay Worker started.");

            await _consumer.ConsumeAsync(
                async message =>
                {
                    using var scope =
                        _scopeFactory.CreateScope();

                    var useCase =
                        scope.ServiceProvider
                            .GetRequiredService<ProcessTransferUseCase>();

                    await useCase.ExecuteAsync(
                        message,
                        stoppingToken);

                    _logger.LogInformation(
                        "Transfer {TransactionId} processed.",
                        message.TransactionId);
                },
                stoppingToken);
        }
    }
}

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using WalletPay.Application.Messaging;

namespace WalletPay.Infrastructure.Messaging.RabbitMQ
{
    public class RabbitMqTransferConsumer
    {
        private readonly RabbitMqSettings _settings;

        public RabbitMqTransferConsumer(
            RabbitMqSettings settings)
        {
            _settings = settings;
        }

        public async Task ConsumeAsync(
            Func<TransferCreatedMessage, Task> handler,
            CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            await using var connection =
                await factory.CreateConnectionAsync(
                    cancellationToken);

            await using var channel =
                await connection.CreateChannelAsync(
                    cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                queue: "transfer-created",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, eventArgs) =>
            {
                try
                {
                    var json = System.Text.Encoding.UTF8.GetString(
                        eventArgs.Body.Span);

                    var message =
                        JsonSerializer.Deserialize<TransferCreatedMessage>(
                            json);

                    if (message is null)
                    {
                        await channel.BasicNackAsync(
                            eventArgs.DeliveryTag,
                            multiple: false,
                            requeue: false);

                        return;
                    }

                    await handler(message);

                    await channel.BasicAckAsync(
                        eventArgs.DeliveryTag,
                        multiple: false);
                }
                catch
                {
                    await channel.BasicNackAsync(
                        eventArgs.DeliveryTag,
                        multiple: false,
                        requeue: true);
                }
            };

            await channel.BasicConsumeAsync(
                queue: "transfer-created",
                autoAck: false,
                consumer: consumer);

            await Task.Delay(
                Timeout.Infinite,
                cancellationToken);
        }
    }
}

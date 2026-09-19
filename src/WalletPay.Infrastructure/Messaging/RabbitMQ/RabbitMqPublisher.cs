using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using WalletPay.Application.Interfaces;

namespace WalletPay.Infrastructure.Messaging.RabbitMQ
{
    public class RabbitMqPublisher : IMessagePublisher
    {
        private readonly RabbitMqSettings _settings;

        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMqPublisher(RabbitMqSettings settings)
        {
            _settings = settings;
        }

        private async Task<IChannel> GetChannelAsync(
        CancellationToken cancellationToken)
        {
            if (_channel is not null)
                return _channel;

            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            _connection = await factory.CreateConnectionAsync(
                cancellationToken);

            _channel = await _connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

            return _channel;
        }

        public async Task PublishAsync<T>(
            T message,
            string queue,
            CancellationToken cancellationToken = default)
        {
            var channel = await GetChannelAsync(cancellationToken);

            await channel.QueueDeclareAsync(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            var json = JsonSerializer.Serialize(message);

            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent
            };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queue,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);
        }
    }
}

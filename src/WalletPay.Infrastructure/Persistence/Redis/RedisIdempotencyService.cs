using StackExchange.Redis;
using WalletPay.Application.Interfaces;

namespace WalletPay.Infrastructure.Persistence.Redis
{
    public class RedisIdempotencyService : IIdempotencyService
    {
        private readonly IConnectionMultiplexer _redis;

        public RedisIdempotencyService(
            IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<bool> HasBeenProcessedAsync(
            string key,
            CancellationToken cancellationToken = default)
        {
            var database = _redis.GetDatabase();

            return await database.KeyExistsAsync(key);
        }

        public async Task MarkAsProcessedAsync(
            string key,
            TimeSpan expiration,
            CancellationToken cancellationToken = default)
        {
            var database = _redis.GetDatabase();

            await database.StringSetAsync(
                key,
                "processed",
                expiration);
        }
    }
}

using MongoDB.Driver;

namespace WalletPay.Infrastructure.Persistence.MongoDb
{
    public class MongoSessionContext
    {
        public IClientSessionHandle? Session { get; set; }
    }
}

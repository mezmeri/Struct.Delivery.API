using StackExchange.Redis;
using WebhookReceiver.Interfaces;

namespace WebhookReceiver.Managers
{
    public class DbQueueManager : IDbQueueManager
    {
        private readonly IDatabase _database;
        private const string ProductUpdateQueueName = "products:updates:pending";

        public DbQueueManager(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public Task EnqueueUpdateAsync(string id)
        {
            return _database.SetAddAsync(ProductUpdateQueueName, id);
        }
    }
}

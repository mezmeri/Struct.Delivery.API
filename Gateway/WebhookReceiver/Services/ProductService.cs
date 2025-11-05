using WebhookReceiver.Interfaces;

namespace WebhookReceiver.Services
{
    public class ProductService
    {
        private readonly IDbQueueManager _queueManager;

        public ProductService(IDbQueueManager queueManager)
        {
            _queueManager = queueManager;
        }

        public Task EnqueueUpdateAsync(string id)
        {
            return _queueManager.EnqueueUpdateAsync(id);
        }
    }
}

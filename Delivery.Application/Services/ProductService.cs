using Delivery.Application.Interfaces;

namespace Delivery.Application.Services
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
        public Task EnqueueUpdatesAsync(IEnumerable<string> productIds)
        {
            return _queueManager.EnqueueUpdatesAsync(productIds);
        }
    }
}

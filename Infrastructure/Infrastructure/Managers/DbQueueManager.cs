using Delivery.Application.Interfaces;
namespace Delivery.Infrastructure.Managers
{
    public class DbQueueManager : IDbQueueManager
    {
        private readonly IProductWriteRepository _productWriteRepository;

        public DbQueueManager(IProductWriteRepository productWriteRepository)
        {
            _productWriteRepository = productWriteRepository;
        }

        public Task EnqueueUpdateAsync(string id)
        {
            return _productWriteRepository.AddToQueueAsync(id);
        }

        public Task EnqueueUpdatesAsync(IEnumerable<string> ids)
        {
            return _productWriteRepository.AddToQueueAsync(ids);
        }

    }
}

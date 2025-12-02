using Delivery.Application.Models;

namespace Delivery.Application.Interfaces.Managers
{
    public interface IQueueManager
    {
        //Task EnqueueUpdatesAsync(IEnumerable<string> ids);
        Task EnqueueUpdatesAsync(IEnumerable<ProductChangeQueueItem> changes);

    }
}

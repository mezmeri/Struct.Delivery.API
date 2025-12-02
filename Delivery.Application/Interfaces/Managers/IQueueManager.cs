using Delivery.Domain.Events;

namespace Delivery.Application.Interfaces.Managers
{
    public interface IQueueManager
    {
        Task EnqueueUpdatesAsync(IEnumerable<QueueItemEventArgs> events);

    }
}

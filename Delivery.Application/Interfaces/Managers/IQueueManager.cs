using Delivery.Application.Models;

namespace Delivery.Application.Interfaces.Managers
{
    public interface IQueueManager
    {
        //Task EnqueueUpdatesAsync(IEnumerable<string> ids);
        
        // For test af productchanges
        Task EnqueueEntityUpdatesAsync(IEnumerable<EntityItem> changes);

    }
}

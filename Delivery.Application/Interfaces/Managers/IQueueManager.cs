namespace Delivery.Application.Interfaces.Managers
{
    public interface IQueueManager
    {
        Task EnqueueUpdatesAsync(IEnumerable<string> ids);

    }
}

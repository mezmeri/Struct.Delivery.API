namespace Delivery.Application.Interfaces.Managers
{
    public interface IQueueManager
    {
        Task EnqueueUpdatesAsync(string eventType, IEnumerable<string> ids);

    }
}

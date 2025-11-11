namespace Delivery.Application.Interfaces.Managers
{
    public interface IDbQueueManager
    {
        Task EnqueueUpdatesAsync(IEnumerable<string> ids);

    }
}

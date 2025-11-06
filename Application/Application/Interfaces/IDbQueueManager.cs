namespace Application.Interfaces
{
    public interface IDbQueueManager
    {
        Task EnqueueUpdateAsync(string id);
        Task EnqueueUpdatesAsync(IEnumerable<string> ids);

    }
}

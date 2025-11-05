namespace WebhookReceiver.Interfaces
{
    public interface IDbQueueManager
    {
        Task EnqueueUpdateAsync(string id);
    }
}

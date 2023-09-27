namespace Foundation.BackgroundTasks
{
    public interface IBackgroundTask
    {
        Task DoWorkAsync(CancellationToken stoppingToken);
    }
}

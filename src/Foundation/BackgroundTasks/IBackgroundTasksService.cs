namespace Foundation.BackgroundTasks
{
    public interface IBackgroundTasksService
    {
        public Task AddTaskAsync(IBackgroundTask task);
        public Task RemoveTaskAsync(IBackgroundTask task);
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Foundation.BackgroundTasks
{
    public class BackgroundTaskService : IHostedService, IBackgroundTasksService
    {
        private readonly Dictionary<IBackgroundTask, (CancellationTokenSource, Task)> _tasks = new Dictionary<IBackgroundTask, (CancellationTokenSource, Task)>();

        private readonly IServiceProvider _serviceProvider;

        public BackgroundTaskService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task AddTaskAsync(IBackgroundTask task)
        {
            if (!_tasks.ContainsKey(task))
            {
                var cts = new CancellationTokenSource();
                var executionTask = task.DoWorkAsync(cts.Token);

                _tasks.Add(task, (cts, executionTask));

                await executionTask;

                cts.Cancel();
                _tasks.Remove(task);
            }
        }

        public async Task RemoveTaskAsync(IBackgroundTask task)
        {
            if (_tasks.ContainsKey(task))
            {
                _tasks[task].Item1.Cancel();
                await _tasks[task].Item2;
                _tasks.Remove(task);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var tasks = _serviceProvider.GetRequiredService<IEnumerable<IBackgroundTask>>();

            foreach (var task in tasks)
            {
                AddTaskAsync(task);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var cts in _tasks.Values.Select(f => f.Item1))
            {
                cts.Cancel();
            }

            return Task.WhenAll(_tasks.Values.Select(f => f.Item2));
        }
    }
}

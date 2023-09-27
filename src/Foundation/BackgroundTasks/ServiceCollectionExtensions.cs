using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Foundation.BackgroundTasks
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBackgroundTasks(this IServiceCollection services)
        {
            services.AddSingleton<BackgroundTaskService>();
            services.AddSingleton<IHostedService>(f => f.GetRequiredService<BackgroundTaskService>());

            services.AddSingleton<IBackgroundTasksService>(f => f.GetRequiredService<BackgroundTaskService>());

            return services;
        }


    }
}

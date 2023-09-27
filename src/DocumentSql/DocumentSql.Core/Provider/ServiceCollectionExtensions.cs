using DocumentSql;

using Microsoft.Extensions.DependencyInjection;

using System;

namespace DocumentSql.Provider
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDbProvider(this IServiceCollection services, Action<IConfiguration> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            var config = new Configuration();
            setupAction.Invoke(config);
            services.AddSingleton(StoreFactory.CreateAsync(config).GetAwaiter().GetResult());

            return services;
        }
    }
}

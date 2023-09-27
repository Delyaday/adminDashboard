using DocumentSql;
using DocumentSql.Indexes;
using DocumentSql.Provider.Sqlite;

using Foundation.Data.Migrations;

using Microsoft.Extensions.DependencyInjection;

using System.Data;

namespace Foundation.Data
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataAccess(this IServiceCollection services)
        {
            services.AddScoped<IDataMigrationManager, DataMigrationManager>();

            services.AddSingleton(sp =>
            {
                IConfiguration storeConfiguration = new Configuration();

                storeConfiguration
                  .UseSqLite($"Data Source=foundation.db;Cache=Shared", IsolationLevel.ReadUncommitted)
                  .UseDefaultIdGenerator();

                var store = StoreFactory.CreateAsync(storeConfiguration).GetAwaiter().GetResult();
                var indexes = sp.GetServices<IIndexProvider>();

                store.RegisterIndexes(indexes);

                return store;
            });

            services.AddScoped(sp =>
            {
                var store = sp.GetService<IStore>();

                if (store == null)
                {
                    return null;
                }

                var session = store.CreateSession();

                return session;
            });

            return services;
        }
    }
}

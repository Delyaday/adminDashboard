using DocumentSql.Indexes;

using Foundation.Data.Migrations;
using Foundation.Users.Authentication;
using Foundation.Users.Records;
using Foundation.Users.Services;

using Microsoft.Extensions.DependencyInjection;

namespace Foundation.Users
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUsers(this IServiceCollection services)
        {
            services.AddSingleton<IIndexProvider, UserRecordIndexProvider>();
            services.AddSingleton<IDataMigration, Migrations>();

            services.AddScoped<IJwtUtils, JwtUtils>();
            services.AddScoped<IUsersService, UsersService>();

            return services;
        }
    }
}

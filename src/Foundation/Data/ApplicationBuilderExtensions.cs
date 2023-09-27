using Foundation.Data.Migrations;
using Foundation.Users;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation.Data
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseDataAccess(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var migrationManager = scope.ServiceProvider.GetRequiredService<IDataMigrationManager>();

                migrationManager.UpdateAllAsync().Wait();
            }

            return app;
        }
    }
}

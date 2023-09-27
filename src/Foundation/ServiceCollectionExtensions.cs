using Foundation.Data;
using Foundation.Users;

using System.Text.Json.Serialization;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFoundation(this IServiceCollection services)
        {
            services.AddUsers();
            services.AddDataAccess();

            services.AddControllers()
                .AddNewtonsoftJson()
                .AddJsonOptions(x => x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

            return services;
        }
    }
}

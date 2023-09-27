using Foundation.Data;
using Foundation.Users.Authorization;
using Foundation.Utils;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseFoundation(this IApplicationBuilder app)
        {
            app.UseCors(opts =>
            {
                opts.SetIsOriginAllowed(_ => true);

                opts.AllowAnyHeader();
                opts.AllowAnyMethod();
                opts.AllowCredentials();
            });

            app.UseMiddleware<ErrorHandlerMiddleware>();
            app.UseMiddleware<JwtMiddleware>();
            //app.UseMiddleware<CorsMiddleware>();

            app.UseDataAccess();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            return app;
        }
    }
}

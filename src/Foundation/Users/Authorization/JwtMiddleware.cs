using Foundation.Users.Authentication;
using Foundation.Users.Services;

using Microsoft.AspNetCore.Http;

namespace Foundation.Users.Authorization
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next=next;
        }

        public async Task Invoke(HttpContext context, IUsersService usersService, IJwtUtils jwtUtils)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = jwtUtils.ValidateJwtToken(token);
            if (userId != null)
            {
                var user = await usersService.GetByIdAsync(userId.Value);

                context.Items["User"] = user;
            }

            await _next(context);
        }
    }
}

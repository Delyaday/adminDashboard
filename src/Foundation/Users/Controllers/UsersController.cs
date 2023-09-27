using Foundation.Users.Authentication;
using Foundation.Users.Authorization;
using Foundation.Users.Requests;
using Foundation.Users.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Users.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly IUsersService _usersService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="usersService"></param>
        public UsersController(IUsersService usersService)
        {
            _usersService = usersService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IEnumerable<User>> GetAll()
        {
            return await _usersService.GetAllAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:int}")]
        public async Task<User> GetById(int id)
        {
            return await _usersService.GetByIdAsync(id);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        [HttpGet("{login}")]
        public async Task<User> GetByLogin(string login)
        {
            return await _usersService.GetByLoginAsync(login);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("password/change")]
        public async Task ChangePassword(ChangePasswordRequest request)
        {
            await _usersService.ChangeUserPasswordAsync(request);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("create")]
        public async Task<User> CreateUser(CreateUserRequest request)
        {
            return await _usersService.CreateUserAsync(request);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("update")]
        public async Task<User> UpdateUser(User user)
        {
            return await _usersService.UpdateUserAsync(user);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:int}")]
        public async Task Delete(int id)
        {
            await _usersService.DeleteUserAsync(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [AllowAnonymous]
        [HttpPost("create_default_super_user")]
        public async Task InitializeDefaultSuperUser(CreateUserRequest request)
        {
            var users = await _usersService.GetAllAsync();
            if (!users.Any())
            {
                await _usersService.CreateUserAsync(request);
            }
            else
            {
                throw new Exception("Нука сдёрни отсюда хакерюга проклятый!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate(AuthenticateRequest request)
        {
            var response = await _usersService.AuthenticateAsync(request, GetCurrentIpAddress());
            SetTokenCookie(response.RefreshToken);
            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("token/refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var response = await _usersService.RefreshTokenAsync(refreshToken, GetCurrentIpAddress());
            SetTokenCookie(response.RefreshToken);
            return Ok(response);
        }

        [HttpPost("token/revoke")]
        public async Task<IActionResult> RevokeToken(RevokeTokenRequest request)
        {
            var token = request.Token ?? Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Token is required" });

            await _usersService.RevokeTokenAsync(token, GetCurrentIpAddress());
            return Ok(new { message = "Token revoked" });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        private void SetTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetCurrentIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}

using BCrypt.Net;

using DocumentSql;

using Foundation.Users.Authentication;
using Foundation.Users.Records;
using Foundation.Users.Requests;
using Foundation.Utils;

using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ApplicationException = Foundation.Utils.ApplicationException;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Foundation.Users.Services
{
    public interface IUsersService
    {
        Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest model, string ipAddress);
        Task<AuthenticateResponse> RefreshTokenAsync(string token, string ipAddress);
        Task RevokeTokenAsync(string token, string ipAddress);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> GetByIdAsync(int id);
        Task<User> GetByLoginAsync(string login);
        Task<User> CreateUserAsync(CreateUserRequest request);
        Task<User> UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);

        Task ChangeUserPasswordAsync(ChangePasswordRequest request);
    }

    public class UsersService : IUsersService
    {
        private readonly IStore _store;
        private readonly IJwtUtils _jwtUtils;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="store"></param>
        /// <param name="jwtUtils"></param>
        /// <param name="configuration"></param>
        public UsersService(IStore store, IJwtUtils jwtUtils, IConfiguration configuration)
        {
            _store = store;
            _jwtUtils = jwtUtils;
            _configuration = configuration;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            using var session = _store.CreateSession();

            var users = await session.Query<UserRecord>().ListAsync();

            return users.Select(RecordToUser);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<User> GetByIdAsync(int id)
        {
            using var session = _store.CreateSession();

            var user = await session.GetAsync<UserRecord>(id);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            return RecordToUser(user);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<User> GetByLoginAsync(string login)
        {
            using var session = _store.CreateSession();

            var user = await session.Query<UserRecord, UserRecordIndex>().Where(f => f.Login == login).FirstOrDefaultAsync();
            if (user == null)
                throw new KeyNotFoundException("User not found");

            return RecordToUser(user);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<User> CreateUserAsync(CreateUserRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(request.Login))
                throw new ArgumentNullException(nameof(request.Login));

            if (string.IsNullOrEmpty(request.Password))
                throw new ArgumentNullException(nameof(request.Password));

            using var session = _store.CreateSession();

            var userRecord = new UserRecord
            {
                Login = request.Login,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            session.Save(userRecord);

            await session.CommitAsync();

            return RecordToUser(userRecord);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<User> UpdateUserAsync(User user)
        {
            using var session = _store.CreateSession();

            var userRecord = await session.GetAsync<UserRecord>(user.Id);

            if (userRecord == null)
                throw new KeyNotFoundException("User not found");

            userRecord.FirstName = user.FirstName;
            userRecord.LastName = user.LastName;

            session.Save(userRecord);

            return RecordToUser(userRecord);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task ChangeUserPasswordAsync(ChangePasswordRequest request)
        {
            using var session = _store.CreateSession();

            var userRecord = await session.Query<UserRecord, UserRecordIndex>().Where(f => f.Login == request.Login).FirstOrDefaultAsync();
            if (userRecord == null)
                throw new KeyNotFoundException("User not found");

            userRecord.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteUserAsync(int id)
        {
            using var session = _store.CreateSession();

            var user = await session.GetAsync<UserRecord>(id);
            if (user != null)
            {
                session.Delete(user);

                await session.CommitAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public async Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request, string ipAddress)
        {
            using var session = _store.CreateSession();

            var user = await session.Query<UserRecord, UserRecordIndex>().Where(f => f.Login == request.Login).FirstOrDefaultAsync();

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new ApplicationException("Username or password is incorrect");

            var jwtToken = _jwtUtils.GenerateJwtToken(user.Id);
            var refreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);

            user.RefreshTokens.Add(refreshToken);

            RemoveOldRefreshTokens(user);

            session.Save(user);

            return new AuthenticateResponse(RecordToUser(user), jwtToken, refreshToken.Token);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public async Task<AuthenticateResponse> RefreshTokenAsync(string token, string ipAddress)
        {
            var user = await GetUserByRefreshTokenAsync(token);
            var refreshToken = user.RefreshTokens.Single(f => f.Token == token);

            using var session = _store.CreateSession();

            if (refreshToken.IsRevoked)
            {
                RevokeDescendantRefreshTokens(refreshToken, user, ipAddress, $"Attempted reuse of revoked ancestor token: {token}");
                session.Save(user);
            }

            if (!refreshToken.IsActive)
                throw new ApplicationException("Invalid token");

            var newRefreshToken = RotateRefreshToken(refreshToken, ipAddress);
            user.RefreshTokens.Add(newRefreshToken);

            RemoveOldRefreshTokens(user);

            session.Save(user);

            var jwtToken = _jwtUtils.GenerateJwtToken(user.Id);

            return new AuthenticateResponse(RecordToUser(user), jwtToken, newRefreshToken.Token);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public async Task RevokeTokenAsync(string token, string ipAddress)
        {
            var user = await GetUserByRefreshTokenAsync(token);
            using var session = _store.CreateSession();

            var refreshToken = user.RefreshTokens.Single(f => f.Token == token);

            if (!refreshToken.IsActive)
                throw new ApplicationException("Invalid token");

            RevokeRefreshToken(refreshToken, ipAddress, "Revoked without replacement");

            session.Save(user);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        private async Task<UserRecord> GetUserByRefreshTokenAsync(string token)
        {
            using var session = _store.CreateSession();

            var users = await session.Query<UserRecord>().ListAsync();

            var user = users.SingleOrDefault(f => f.RefreshTokens.Any(r => r.Token == token));

            if (user == null)
                throw new ApplicationException("Invalid token");

            return user;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        private RefreshToken RotateRefreshToken(RefreshToken refreshToken, string ipAddress)
        {
            var newRefreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);

            RevokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);

            return newRefreshToken;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <param name="user"></param>
        /// <param name="ipAddress"></param>
        /// <param name="reason"></param>
        private void RevokeDescendantRefreshTokens(RefreshToken refreshToken, UserRecord user, string ipAddress, string reason)
        {
            if (!string.IsNullOrEmpty(refreshToken.ReplacedByToken))
            {
                var childToken = user.RefreshTokens.SingleOrDefault(f => f.Token == refreshToken.ReplacedByToken);

                if (childToken.IsActive)
                {
                    RevokeRefreshToken(childToken, ipAddress, reason);
                }
                else
                {
                    RevokeDescendantRefreshTokens(childToken, user, ipAddress, reason);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="ipAddress"></param>
        /// <param name="reason"></param>
        /// <param name="replacedByToken"></param>
        private void RevokeRefreshToken(RefreshToken token, string ipAddress, string reason = null, string replacedByToken = null)
        {
            token.Revoked = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.ReasonRevoked = reason;
            token.ReplacedByToken = replacedByToken;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        private void RemoveOldRefreshTokens(UserRecord user)
        {
            var settings = _configuration.GetSection("Authentication").Get<AuthenticationSettings>();

            user.RefreshTokens.RemoveAll(f =>
                !f.IsActive &&
                f.Created.AddDays(settings.RefreshTokenTTL) <= DateTime.UtcNow);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        private User RecordToUser(UserRecord record)
        {
            return new User
            {
                Id = record.Id,
                Login = record.Login,
                FirstName = record.FirstName,
                LastName = record.LastName
            };
        }
    }
}

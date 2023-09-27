using DocumentSql;
using SailorMoon.Web.Records;
using ISession = DocumentSql.ISession;

namespace SailorMoon.Web.Services
{
    public interface IUserDescriptionService
    {
        Task<IEnumerable<UserDescriptionRecord>> Get();
        Task<UserDescriptionRecord> Get(int id);
        Task<UserDescriptionRecord> Get(string login);
        Task<UserDescriptionRecord> Create(UserDescriptionRecord record);
        Task<UserDescriptionRecord> Update(UserDescriptionRecord source);
        Task Delete(int id);
        Task Delete(string login);
    }

    public class UserDescriptionService : IUserDescriptionService
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        public UserDescriptionService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<UserDescriptionRecord>> Get()
        {
            using var session = _serviceProvider.GetRequiredService<ISession>();

            return await session.Query<UserDescriptionRecord, UserDescriptionRecordIndex>().ListAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<UserDescriptionRecord> Get(int id)
        {
            using var session = _serviceProvider.GetRequiredService<ISession>();

            return await session.GetAsync<UserDescriptionRecord>(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public async Task<UserDescriptionRecord> Get(string login)
        {
            using var session = _serviceProvider.GetRequiredService<ISession>();

            return await session.Query<UserDescriptionRecord, UserDescriptionRecordIndex>().Where(f => f.Login == login).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public async Task<UserDescriptionRecord> Create(UserDescriptionRecord record)
        {
            using var session = _serviceProvider.GetRequiredService<ISession>();

            session.Save(record);

            return record;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<UserDescriptionRecord> Update(UserDescriptionRecord source)
        {
            using var session = _serviceProvider.GetRequiredService<ISession>();

            var target = await session.GetAsync<UserDescriptionRecord>(source.Id);

            if (target == null)
                throw new NullReferenceException(nameof(target));

            Copy(source, target);

            session.Save(target);

            return target;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task Delete(int id)
        {
            using var session = _serviceProvider.GetRequiredService<ISession>();

            var record = await session.GetAsync<UserDescriptionRecord>(id);

            if (record != null)
                session.Delete(record);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public async Task Delete(string login)
        {
            using var session = _serviceProvider.GetRequiredService<ISession>();

            var record = await session.Query<UserDescriptionRecord, UserDescriptionRecordIndex>().Where(f => f.Login == login).FirstOrDefaultAsync();

            if (record != null)
                session.Delete(record);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        private void Copy(UserDescriptionRecord source, UserDescriptionRecord target)
        {
            target.Phone = source.Phone;
            target.Login = source.Login;
            target.Position = source.Position;
            target.AccessLevel = source.AccessLevel;
            target.Description = source.Description;
        }
    }
}

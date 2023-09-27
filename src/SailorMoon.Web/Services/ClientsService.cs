using DocumentSql;
using SailorMoon.Web.Records;
using ISession = DocumentSql.ISession;

namespace SailorMoon.Web.Services
{
    public interface IClientsService
    {
        Task<IEnumerable<ClientRecord>> Get();
        Task<ClientRecord> Get(int id);
        Task<ClientRecord> Get(string phone);
        Task<ClientRecord> Create(ClientRecord record);
        Task<ClientRecord> Update(ClientRecord source);
        Task Delete(int id);
        Task Delete(string phone);
    }

    public class ClientsService : IClientsService
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        public ClientsService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ClientRecord>> Get()
        {
            using var session = _serviceProvider.GetRequiredService<ISession>();

            return await session.Query<ClientRecord, ClientRecordIndex>().ListAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ClientRecord> Get(int id)
        {
            using var session = _serviceProvider.GetRequiredService<ISession>();

            return await session.GetAsync<ClientRecord>(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public async Task<ClientRecord> Get(string phone)
        {
            using var session = _serviceProvider.GetRequiredService<ISession>();

            return await session.Query<ClientRecord, ClientRecordIndex>().Where(f => f.Phone == phone).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public async Task<ClientRecord> Create(ClientRecord record)
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
        public async Task<ClientRecord> Update(ClientRecord source)
        {
            using var session = _serviceProvider.GetRequiredService<ISession>();

            var target = await session.GetAsync<ClientRecord>(source.Id);

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

            var record = await session.GetAsync<ClientRecord>(id);

            if (record != null)
                session.Delete(record);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public async Task Delete(string phone)
        {
            using var session = _serviceProvider.GetRequiredService<ISession>();

            var record = await session.Query<ClientRecord, ClientRecordIndex>().Where(f => f.Phone == phone).FirstOrDefaultAsync();

            if (record != null)
                session.Delete(record);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        private void Copy(ClientRecord source, ClientRecord target)
        {
            target.Phone = source.Phone;
            target.FirstName = source.FirstName;
            target.LastName = source.LastName;
            target.Birthday = source.Birthday;
            target.Description = source.Description;
        }
    }
}

using DocumentSql;
using SailorMoon.Web.Records;
using ISession = DocumentSql.ISession;

namespace SailorMoon.Web.Services
{
    public interface IServicesService
    {
        Task<IEnumerable<ServiceRecord>> Get();
        Task<ServiceRecord> Get(int id);
        Task<ServiceRecord> Create(ServiceRecord record);
        Task<ServiceRecord> Update(ServiceRecord source);
        Task Delete(int id);
    }

    public class ServicesService : IServicesService
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        public ServicesService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ServiceRecord>> Get()
        {
            using var session = _serviceProvider.GetRequiredService<ISession>();

            return await session.Query<ServiceRecord>().ListAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ServiceRecord> Get(int id)
        {
            using var session = _serviceProvider.GetRequiredService<ISession>();

            return await session.GetAsync<ServiceRecord>(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public async Task<ServiceRecord> Create(ServiceRecord record)
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
        public async Task<ServiceRecord> Update(ServiceRecord source)
        {
            using var session = _serviceProvider.GetRequiredService<ISession>();

            var target = await session.GetAsync<ServiceRecord>(source.Id);

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

            var record = await session.GetAsync<ServiceRecord>(id);

            if (record != null)
                session.Delete(record);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        private void Copy(ServiceRecord source, ServiceRecord target)
        {
            target.Title = source.Title;
            target.Category = source.Category;
            target.Duration = source.Duration;
            target.Price = source.Price;
            target.Description = source.Description;
        }
    }
}

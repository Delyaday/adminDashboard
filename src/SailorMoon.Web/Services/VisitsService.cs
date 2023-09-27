using DocumentSql;

using SailorMoon.Web.Records;

using ISession = DocumentSql.ISession;

namespace SailorMoon.Web.Services
{
    public interface IVisitsService
    {
        Task<IEnumerable<VisitRecord>> Get();
        Task<VisitRecord> Get(int id);
        Task<IEnumerable<VisitRecord>> GetByClient(int clientId);
        Task<IEnumerable<VisitRecord>> GetByService(int serviceId);
        Task<IEnumerable<VisitRecord>> GetByStaff(int staffId);
        Task<VisitRecord> Create(VisitRecord record);
        Task<VisitRecord> Update(VisitRecord source);
        Task Delete(int id);
    }

    public class VisitsService : IVisitsService
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        public VisitsService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<VisitRecord>> Get()
        {
            using var session = _serviceProvider.GetRequiredService<ISession>();

            return await session.Query<VisitRecord, VisitRecordIndex>().ListAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<VisitRecord> Get(int id)
        {
            using var session = _serviceProvider.GetRequiredService<ISession>();

            return await session.GetAsync<VisitRecord>(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<VisitRecord>> GetByClient(int clientId)
        {
            using var session = _serviceProvider.GetRequiredService<ISession>();

            return await session.Query<VisitRecord, VisitRecordIndex>().Where(f => f.ClientId == clientId).ListAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<VisitRecord>> GetByService(int serviceId)
        {
            using var session = _serviceProvider.GetRequiredService<ISession>();

            return await session.Query<VisitRecord, VisitRecordIndex>().Where(f => f.ServiceId == serviceId).ListAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<VisitRecord>> GetByStaff(int staffId)
        {
            using var session = _serviceProvider.GetRequiredService<ISession>();

            return await session.Query<VisitRecord, VisitRecordIndex>().Where(f => f.StaffId == staffId).ListAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public async Task<VisitRecord> Create(VisitRecord record)
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
        public async Task<VisitRecord> Update(VisitRecord source)
        {
            using var session = _serviceProvider.GetRequiredService<ISession>();

            var target = await session.GetAsync<VisitRecord>(source.Id);

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

            var record = await session.GetAsync<VisitRecord>(id);

            if (record != null)
                session.Delete(record);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        private void Copy(VisitRecord source, VisitRecord target)
        {
            target.ClientId = source.ClientId;
            target.ServiceId = source.ServiceId;
            target.StaffId = source.StaffId;
            target.Time = source.Time;
            target.Description = source.Description;
        }
    }
}

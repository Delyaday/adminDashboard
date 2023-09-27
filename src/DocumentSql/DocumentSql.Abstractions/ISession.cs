using DocumentSql.Indexes;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace DocumentSql
{
    public interface ISession : IDisposable
    {
        void Save(object obj, bool checkConcurrency = false);

        void Delete(object item);

        bool Import(object item, int id);

        void Detach(object item);

        Task<IEnumerable<T>> GetAsync<T>(int[] ids) where T : class;

        IQuery Query();

        IQuery<T> ExecuteQuery<T>(ICompiledQuery<T> compiledQuery) where T : class;

        void Cancel();

        Task FlushAsync();

        Task CommitAsync();

        Task<DbTransaction> DemandAsync();

        ISession RegisterIndexes(params IIndexProvider[] indexProviders);

        IStore Store { get; }
    }

    public static class SessionExtensions
    {
        public async static Task<T> GetAsync<T>(this ISession session, int id) where T : class
        {
            return (await session.GetAsync<T>(new[] { id })).FirstOrDefault();
        }

        public static bool Import(this ISession session, object item)
        {
            return session.Import(item, 0);
        }
    }
}

using DocumentSql.Indexes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DocumentSql
{
    public interface IStore : IDisposable
    {
        ISession CreateSession(IsolationLevel isolationLevel);
        IStore RegisterIndexes(IEnumerable<IIndexProvider> indexProviders);
        IConfiguration Configuration { get; }
        Task InitializeCollectionAsync(string collection);
        IIdAccessor<int> GetIdAccessor(Type tContainer, string name);
        IEnumerable<IndexDescriptor> Describe(Type target);
        ISqlDialect Dialect { get; }
        ITypeService TypeNames { get; }
    }

    public static class StoreExtensions
    {
        public static ISession CreateSession(this IStore store)
        {
            return store.CreateSession(store.Configuration.IsolationLevel);
        }
    }
}

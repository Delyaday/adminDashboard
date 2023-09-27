using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace DocumentSql
{
    public interface IIdGenerator
    {        
        Task InitializeAsync(IStore store, ISchemaBuilder builder);

        Task InitializeCollectionAsync(IConfiguration configuration, string collection);

        long GetNextId(string collection);
    }
}

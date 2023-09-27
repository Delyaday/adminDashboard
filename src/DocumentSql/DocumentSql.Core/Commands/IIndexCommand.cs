using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DocumentSql.Commands
{
    public interface IIndexCommand
    {
        Task ExecuteAsync(DbConnection connection, DbTransaction transaction, ISqlDialect dialect, ILogger logger);
        int ExecutionOrder { get; }
    }
}

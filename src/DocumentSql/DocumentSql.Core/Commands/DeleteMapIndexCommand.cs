using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace DocumentSql.Commands
{
    public class DeleteMapIndexCommand : IIndexCommand
    {
        private readonly int _documentId;
        private readonly Type _indexType;
        private readonly string _tablePrefix;

        public int ExecutionOrder { get; } = 1;

        public DeleteMapIndexCommand(Type indexType, int documentId, string tablePrefix, ISqlDialect dialect)
        {
            _indexType = indexType;
            _documentId = documentId;
            _tablePrefix = tablePrefix;
        }

        public Task ExecuteAsync(DbConnection connection, DbTransaction transaction, ISqlDialect dialect, ILogger logger)
        {
            var command = "delete from " + dialect.QuoteForTableName(_tablePrefix + _indexType.Name) + " where " + dialect.QuoteForColumnName("DocumentId") + " = " + dialect.QuoteForParameter("Id") + dialect.StatementEnd;
            logger.LogTrace(command);
            return connection.ExecuteAsync(command, new { Id = _documentId }, transaction);
        }
    }
}

using Dapper;
using DocumentSql.Collections;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Threading.Tasks;

namespace DocumentSql.Commands
{
    public class DeleteDocumentCommand : DocumentCommand
    {
        private readonly string _tablePrefix;
        public override int ExecutionOrder { get; } = 4;

        public DeleteDocumentCommand(Document document, string tablePrefix) : base(document)
        {
            _tablePrefix = tablePrefix;
        }

        public override Task ExecuteAsync(DbConnection connection, DbTransaction transaction, ISqlDialect dialect, ILogger logger)
        {
            var documentTable = CollectionHelper.Current.GetPrefixedName(Store.DocumentTable);
            var deleteCmd = "delete from " + dialect.QuoteForTableName(_tablePrefix + documentTable) + " where " + dialect.QuoteForColumnName("Id") + " = " + dialect.QuoteForParameter("Id") + dialect.StatementEnd;
            logger.LogTrace(deleteCmd);
            return connection.ExecuteAsync(deleteCmd, Document, transaction);
        }
    }
}

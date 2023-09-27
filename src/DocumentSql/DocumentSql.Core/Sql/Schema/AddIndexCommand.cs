using DocumentSql.Schema;

namespace DocumentSql.Sql.Schema
{
    public class AddIndexCommand : TableCommand, IAddIndexCommand
    {
        public string IndexName { get; set; }
        public string[] ColumnNames { get; private set; }

        public AddIndexCommand(string tableName, string indexName, params string[] columnNames)
            : base(tableName)
        {
            ColumnNames = columnNames;
            IndexName = indexName;
        }
    }
}

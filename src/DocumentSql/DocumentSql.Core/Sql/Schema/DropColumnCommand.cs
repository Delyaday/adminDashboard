using DocumentSql.Schema;

namespace DocumentSql.Sql.Schema
{
    public class DropColumnCommand : ColumnCommand, IDropColumnCommand
    {
        public DropColumnCommand(string tableName, string columnName)
            : base(tableName, columnName)
        { }
    }
}

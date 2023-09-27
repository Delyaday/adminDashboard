namespace DocumentSql.Schema
{
    public interface IRenameColumnCommand : IColumnCommand
    {
        string NewColumnName { get; }
    }
}

namespace DocumentSql.Schema
{
    public interface IDropIndexCommand : ITableCommand
    {
        string IndexName { get; set; }
    }
}

namespace DocumentSql.Schema
{
    public interface IDropForeignKeyCommand : ISchemaCommand
    {
        string SrcTable { get; }
    }
}

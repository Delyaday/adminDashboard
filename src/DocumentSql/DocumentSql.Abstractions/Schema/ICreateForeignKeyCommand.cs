namespace DocumentSql.Schema
{
    public interface ICreateForeignKeyCommand : ISchemaCommand
    {
        string[] DestColumns { get; }

        string DestTable { get; }

        string[] SrcColumns { get; }

        string SrcTable { get; }
    }
}

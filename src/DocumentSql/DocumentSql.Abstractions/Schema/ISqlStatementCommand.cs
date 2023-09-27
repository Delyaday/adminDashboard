using System.Collections.Generic;

namespace DocumentSql.Schema
{
    public interface ISqlStatementCommand : ISchemaCommand
    {
        string Sql { get; }
        List<string> Providers { get; }
    }
}

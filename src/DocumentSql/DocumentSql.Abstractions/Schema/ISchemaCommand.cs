using System.Collections.Generic;

namespace DocumentSql.Schema
{
    public interface ISchemaCommand
    {
        string Name { get; }
        List<ITableCommand> TableCommands { get; }
    }
}

using DocumentSql.Schema;
using System.Collections.Generic;

namespace DocumentSql.Sql.Schema
{
    public abstract class SchemaCommand : ISchemaCommand
    {
        public string Name { get; private set; }

        public List<ITableCommand> TableCommands { get; private set; }

        public SchemaCommandType Type { get; private set; }

        protected SchemaCommand(string name, SchemaCommandType type)
        {
            TableCommands = new List<ITableCommand>();
            Type = type;
            WithName(name);
        }

        public ISchemaCommand WithName(string name)
        {
            Name = name;
            return this;
        }
    }
}

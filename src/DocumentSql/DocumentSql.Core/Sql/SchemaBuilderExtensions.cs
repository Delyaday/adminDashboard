using DocumentSql.Schema;

using System;
using System.Collections.Generic;

namespace DocumentSql.Sql
{
    public static class SchemaBuilderExtensions
    {
        public static IEnumerable<string> CreateSql(this ICommandInterpreter builder, ISchemaCommand command)
        {
            return builder.CreateSql(new[] { command });
        }        
    }
}

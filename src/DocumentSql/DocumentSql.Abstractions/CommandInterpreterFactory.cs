using System;
using System.Collections.Concurrent;
using System.Data.Common;

namespace DocumentSql
{
    public class CommandInterpreterFactory
    {
        public static readonly ConcurrentDictionary<string, Func<ISqlDialect, ICommandInterpreter>> CommandInterpreters = new ConcurrentDictionary<string, Func<ISqlDialect, ICommandInterpreter>>();

        public static ICommandInterpreter For(DbConnection connection)
        {
            string connectionName = connection.GetType().Name.ToLower();

            if (!CommandInterpreters.ContainsKey(connectionName))
            {
                throw new ArgumentException("Uncnown connection name: " + connectionName);
            }

            var dialect = SqlDialectFactory.For(connection);
            return CommandInterpreters[connectionName](dialect);
        }
    }
}

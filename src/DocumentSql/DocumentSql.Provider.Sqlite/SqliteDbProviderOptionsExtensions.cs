using Microsoft.Data.Sqlite;
using System;
using System.Data;

namespace DocumentSql.Provider.Sqlite
{
    public static class SqliteDbProviderOptionsExtensions
    {
        public static IConfiguration RegisterSqLite(this IConfiguration configuration)
        {
            SqlDialectFactory.SqlDialects["sqliteconnection"] = new SqliteDialect();
            CommandInterpreterFactory.CommandInterpreters["sqliteconnection"] = d => new SqliteCommandInterpreter(d);

            return configuration;
        }

        public static IConfiguration UseInMemory(this IConfiguration configuration)
        {
            const string inMemoryConnectionString = "Data Source=:memory:";
            return UseSqLite(configuration, inMemoryConnectionString, IsolationLevel.Serializable);
        }

        public static IConfiguration UseSqLite(this IConfiguration configuration, string connectionString)
        {
            return UseSqLite(
                configuration,
                connectionString,
                IsolationLevel.Serializable);
        }

        public static IConfiguration UseSqLite(this IConfiguration configuration, string connectionString, IsolationLevel isolationLevel)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (String.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException(nameof(connectionString));
            }

            RegisterSqLite(configuration);
            configuration.ConnectionFactory = new DbConnectionFactory<SqliteConnection>(connectionString);
            configuration.IsolationLevel = isolationLevel;

            return configuration;
        }
    }
}

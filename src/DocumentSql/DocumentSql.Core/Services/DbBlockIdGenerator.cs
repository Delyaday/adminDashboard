using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using DocumentSql.Sql;
using Microsoft.Extensions.Logging;

namespace DocumentSql.Services
{
    /// <summary>
    /// http://literatejava.com/hibernate/linear-block-allocator-a-superior-alternative-to-hilo/
    /// </summary>
    public class DbBlockIdGenerator : IIdGenerator
    {
        private object _synLock = new object();

        public static string TableName => "Identifiers";
        public readonly int MaxRetries = 20;

        private ISqlDialect _dialect;
        private IStore _store;

        private int _blockSize;
        private Dictionary<string, Range> _ranges = new Dictionary<string, Range>();
        private string _tablePrefix;

        private string SelectCommand;
        private string UpdateCommand;
        private string InsertCommand;

        public DbBlockIdGenerator() : this(20)
        {
        }

        public DbBlockIdGenerator(int blockSize)
        {
            _blockSize = blockSize;
        }

        public async Task InitializeAsync(IStore store, ISchemaBuilder builder)
        {
            _dialect = SqlDialectFactory.For(store.Configuration.ConnectionFactory.DbConnectionType);
            _tablePrefix = store.Configuration.TablePrefix;
            _store = store;

            SelectCommand = "SELECT " + _dialect.QuoteForColumnName("nextval") + " FROM " + _dialect.QuoteForTableName(_tablePrefix + TableName) + " WHERE " + _dialect.QuoteForTableName("dimension") + " = " + _dialect.QuoteForParameter("dimension") + _dialect.StatementEnd;
            UpdateCommand = "UPDATE " + _dialect.QuoteForTableName(_tablePrefix + TableName) + " SET " + _dialect.QuoteForColumnName("nextval") + " = " + _dialect.QuoteForParameter("new") + " WHERE " + _dialect.QuoteForColumnName("nextval") + " = " + _dialect.QuoteForParameter("previous") + " AND " + _dialect.QuoteForColumnName("dimension") + " = " + _dialect.QuoteForParameter("dimension") + _dialect.StatementEnd;
            InsertCommand = "INSERT INTO " + _dialect.QuoteForTableName(_tablePrefix + TableName) + " (" + _dialect.QuoteForColumnName("dimension") + ", " + _dialect.QuoteForColumnName("nextval") + ") VALUES(" + _dialect.QuoteForParameter("dimension") + ", " + _dialect.QuoteForParameter("nextval") + ")" + _dialect.StatementEnd;

            using (var connection = store.Configuration.ConnectionFactory.CreateConnection())
            {
                await connection.OpenAsync();

                try
                {
                    using (var transaction = connection.BeginTransaction(store.Configuration.IsolationLevel))
                    {
                        var localBuilder = new SchemaBuilder(store.Configuration, transaction, false);

                        localBuilder.CreateTable(DbBlockIdGenerator.TableName, table => table
                            .Column<string>("dimension", column => column.PrimaryKey().NotNull())
                            .Column<ulong>("nextval")
                            )
                            .AlterTable(DbBlockIdGenerator.TableName, table => table
                                .CreateIndex("IX_Dimension", "dimension")
                            );
                        //TODO Oracle check
                        //.AlterTable(DbBlockIdGenerator.TableName, table => table //FIXME Oracle
                        //    .CreateIndex("IX_Dimension", "dimension")
                        //);
                        transaction.Commit();
                    }
                }
                catch
                {
                }
            }
        }

        public long GetNextId(string collection)
        {
            lock (_synLock)
            {
                collection = String.IsNullOrEmpty(collection) ? _dialect.NullString : collection;

                if (!_ranges.TryGetValue(collection, out var range))
                {
                    throw new InvalidOperationException($"The collection '{collection}' was not initialized");
                }

                var nextId = range.Next();

                if (nextId > range.End)
                {
                    LeaseRange(range);
                    nextId = range.Next();
                }

                return nextId;
            }
        }

        private void LeaseRange(Range range)
        {
            var affectedRows = 0;
            long nextval = 0;
            var retries = 0;

            using (var connection = _store.Configuration.ConnectionFactory.CreateConnection())
            {
                connection.Open();

                do
                {
                    using (var transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            var selectCommand = connection.CreateCommand();
                            selectCommand = (DbCommand)_dialect.ConfigureCommand(selectCommand);
                            selectCommand.CommandText = SelectCommand;

                            var selectDimension = selectCommand.CreateParameter();
                            selectDimension.Value = range.Collection;
                            selectDimension.ParameterName = _dialect.QuoteForParameter("dimension");
                            selectCommand.Parameters.Add(selectDimension);

                            selectCommand.Transaction = transaction;

                            nextval = Convert.ToInt64(selectCommand.ExecuteScalar());

                            var updateCommand = connection.CreateCommand();
                            updateCommand = (DbCommand)_dialect.ConfigureCommand(updateCommand);
                            updateCommand.CommandText = UpdateCommand;

                            var updateDimension = updateCommand.CreateParameter();
                            updateDimension.Value = range.Collection;
                            updateDimension.ParameterName = _dialect.QuoteForParameter("dimension");
                            updateCommand.Parameters.Add(updateDimension);

                            var newValue = updateCommand.CreateParameter();
                            newValue.Value = nextval + _blockSize;
                            newValue.ParameterName = _dialect.QuoteForParameter("new");
                            updateCommand.Parameters.Add(newValue);

                            var previousValue = updateCommand.CreateParameter();
                            previousValue.Value = nextval;
                            previousValue.ParameterName = _dialect.QuoteForParameter("previous");
                            updateCommand.Parameters.Add(previousValue);

                            updateCommand.Transaction = transaction;

                            _store.Configuration.Logger.LogTrace(UpdateCommand);
                            affectedRows = updateCommand.ExecuteNonQuery();

                            transaction.Commit();
                        }
                        catch
                        {
                            affectedRows = 0;
                            transaction.Rollback();
                        }
                    }

                    if (retries++ > MaxRetries)
                    {
                        throw new Exception("Too many retries while trying to lease a range for: " + range.Collection);
                    }

                } while (affectedRows == 0);

                range.SetBlock(nextval, _blockSize);
            }
        }

        public async Task InitializeCollectionAsync(IConfiguration configuration, string collection)
        {
            if (String.IsNullOrEmpty(collection))
            {
                collection = _dialect.NullString;
            }
            if (_ranges.ContainsKey(collection))
            {
                return;
            }

            object nextval;

            using (var connection = configuration.ConnectionFactory.CreateConnection())
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(configuration.IsolationLevel))
                {
                    var selectCommand = transaction.Connection.CreateCommand();
                    selectCommand.CommandText = SelectCommand;

                    var selectDimension = selectCommand.CreateParameter();
                    selectDimension.Value = collection;
                    selectDimension.ParameterName = _dialect.QuoteForParameter("dimension");
                    selectCommand.Parameters.Add(selectDimension);

                    selectCommand.Transaction = transaction;

                    _store.Configuration.Logger.LogTrace(SelectCommand);
                    nextval = await selectCommand.ExecuteScalarAsync();

                    transaction.Commit();
                }

                if (nextval == null)
                {
                    try
                    {
                        using (var transaction = connection.BeginTransaction(configuration.IsolationLevel))
                        {
                            nextval = 1;

                            var command = transaction.Connection.CreateCommand();
                            command.CommandText = InsertCommand;
                            command.Transaction = transaction;

                            var dimensionParameter = command.CreateParameter();
                            dimensionParameter.Value = collection;
                            dimensionParameter.ParameterName = _dialect.QuoteForParameter("dimension");
                            command.Parameters.Add(dimensionParameter);

                            var nextValParameter = command.CreateParameter();
                            nextValParameter.Value = 1;
                            nextValParameter.ParameterName = _dialect.QuoteForParameter("nextval");
                            command.Parameters.Add(nextValParameter);

                            _store.Configuration.Logger.LogTrace(InsertCommand);
                            await command.ExecuteNonQueryAsync();

                            transaction.Commit();
                        }
                    }
                    catch
                    {
                        await InitializeCollectionAsync(configuration, collection);
                    }
                }

                _ranges[collection] = new Range(collection);
            }
        }

        private class Range
        {
            public Range(string collection)
            {
                Collection = collection;
                Cursor = 1;
            }

            public Range SetBlock(long start, int blockSize)
            {
                Start = start;
                End = Start + blockSize - 1;
                Cursor = 0;

                return this;
            }

            public long Next()
            {
                return Start + Cursor++;
            }

            public string Collection;
            public long Cursor;
            public long Start;
            public long End;
        }
    }
}

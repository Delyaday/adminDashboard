﻿using DocumentSql.Schema;
using System.Collections.Generic;

namespace DocumentSql.Sql.Schema
{
    public class SqlStatementCommand : SchemaCommand, ISqlStatementCommand
    {
        protected readonly List<string> _providers;

        public string Sql { get; private set; }
        public List<string> Providers { get { return _providers; } }

        public SqlStatementCommand(string sql)
            : base(string.Empty, SchemaCommandType.SqlStatement)
        {
            Sql = sql;
            _providers = new List<string>();
        }

        public ISqlStatementCommand ForProvider(string dataProvider)
        {
            _providers.Add(dataProvider);
            return this;
        }
    }
}

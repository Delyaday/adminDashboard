using DocumentSql.Schema;
using DocumentSql.Sql;

namespace DocumentSql.Provider.Sqlite
{
    public class SqliteCommandInterpreter : BaseCommandInterpreter
    {
        public SqliteCommandInterpreter(ISqlDialect dialect) : base(dialect)
        { }

        public override IEnumerable<string> Run(ICreateForeignKeyCommand command)
        {
            yield break;
        }

        public override IEnumerable<string> Run(IDropForeignKeyCommand command)
        {
            yield break;
        }
    }
}

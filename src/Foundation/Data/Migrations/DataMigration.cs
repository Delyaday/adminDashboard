using DocumentSql.Sql;

namespace Foundation.Data.Migrations
{
    public interface IDataMigration
    {
        SchemaBuilder SchemaBuilder { get; set; }
    }

    public abstract class DataMigration : IDataMigration
    {
        public SchemaBuilder SchemaBuilder { get; set; }
    }
}

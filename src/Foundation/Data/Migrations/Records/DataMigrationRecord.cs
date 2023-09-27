namespace Foundation.Data.Migrations.Records
{
    public class DataMigrationRecord
    {
        public int Id { get; set; }
        public List<DataMigration> DataMigrations { get; set; }

        public DataMigrationRecord()
        {
            DataMigrations = new List<DataMigration>();
        }
    }

    public class DataMigration
    {
        public string DataMigrationClass { get; set; }
        public int? Version { get; set; }
    }
}

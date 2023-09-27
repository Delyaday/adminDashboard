using Foundation.Data.Migrations;
using Foundation.Users.Records;

namespace Foundation.Users
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder
                .CreateMapIndexTable(nameof(UserRecordIndex), table => table
                .Column<string>(nameof(UserRecordIndex.Login))
            );

            return 1;
        }
    }
}

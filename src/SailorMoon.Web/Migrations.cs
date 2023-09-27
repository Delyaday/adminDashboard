using Foundation.Data.Migrations;

using SailorMoon.Web.Records;

namespace SailorMoon.Web
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder
                .CreateMapIndexTable(nameof(ClientRecordIndex), table => table
                .Column<string>(nameof(ClientRecordIndex.Phone))
                );


            return 1;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder
                .CreateMapIndexTable(nameof(UserDescriptionRecordIndex), table => table
                    .Column<string>(nameof(UserDescriptionRecordIndex.Login))
                    .Column<string>(nameof(UserDescriptionRecordIndex.Phone))
                );


            return 2;
        }

        public int UpdateFrom2()
        {
            SchemaBuilder
                .CreateMapIndexTable(nameof(VisitRecordIndex), table => table
                    .Column<string>(nameof(VisitRecordIndex.ClientId))
                    .Column<string>(nameof(VisitRecordIndex.ServiceId))
                    .Column<string>(nameof(VisitRecordIndex.StaffId))
                );


            return 3;
        }
    }
}

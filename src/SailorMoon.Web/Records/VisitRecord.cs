using DocumentSql.Indexes;

namespace SailorMoon.Web.Records
{
    public class VisitRecord
    {
        public int Id { get; set; }

        public int ClientId { get; set; }

        public int ServiceId { get; set; }

        public int StaffId { get; set; }

        public DateTime Time { get; set; }

        public string Description { get; set; }
    }

    public class VisitRecordIndex : MapIndex
    {
        public int ClientId { get; set; }

        public int ServiceId { get; set; }

        public int StaffId { get; set; }
    }

    public class VisitRecordIndexProvider : IndexProvider<VisitRecord>
    {
        public override void Describe(DescribeContext<VisitRecord> context)
        {
            context.For<VisitRecordIndex>()
                .Map(record =>
                {
                    return new VisitRecordIndex
                    {
                        ClientId = record.ClientId,
                        ServiceId = record.ServiceId,
                        StaffId = record.StaffId,
                    };
                });
        }
    }
}

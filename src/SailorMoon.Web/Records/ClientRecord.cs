using DocumentSql.Indexes;

namespace SailorMoon.Web.Records
{
    public class ClientRecord
    {
        public int Id { get; set; }
        public string Phone { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? Birthday { get; set; }
        public string Description { get; set; }

    }

    public class ClientRecordIndex : MapIndex
    {
        public string Phone { get; set; }
    }

    public class ClientRecordIndexProvider : IndexProvider<ClientRecord>
    {
        public override void Describe(DescribeContext<ClientRecord> context)
        {
            context.For<ClientRecordIndex>()
                .Map(record =>
                {
                    return new ClientRecordIndex
                    {
                        Phone = record.Phone
                    };
                });
        }
    }
}

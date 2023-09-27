using DocumentSql.Indexes;

namespace SailorMoon.Web.Records
{
    public class UserDescriptionRecord
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Phone { get; set; }
        public string Position { get; set; }
        public AccessLevels AccessLevel { get; set; }
        public string Description { get; set; }

    }

    public enum AccessLevels
    {
        Worker,
        Admin,
    }

    public class UserDescriptionRecordIndex : MapIndex
    {
        public string Login { get; set; }
        public string Phone { get; set; }
    }

    public class UserDescriptionRecordIndexProvider : IndexProvider<UserDescriptionRecord>
    {
        public override void Describe(DescribeContext<UserDescriptionRecord> context)
        {
            context.For<UserDescriptionRecordIndex>()
                .Map(record =>
                {
                    return new UserDescriptionRecordIndex
                    {
                        Login = record.Login,
                        Phone = record.Phone
                    };
                });
        }
    }
}

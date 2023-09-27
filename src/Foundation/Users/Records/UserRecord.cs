using DocumentSql.Indexes;

using Foundation.Users.Authentication;

namespace Foundation.Users.Records
{
    public class UserRecord
    {
        public int Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }

    public class UserRecordIndex : MapIndex
    {
        public string Login { get; set; }
    }

    public class UserRecordIndexProvider : IndexProvider<UserRecord>
    {
        public override void Describe(DescribeContext<UserRecord> context)
        {
            context.For<UserRecordIndex>()
                .Map(record =>
                {
                    return new UserRecordIndex
                    {
                        Login= record.Login,
                    };
                });
        }
    }
}

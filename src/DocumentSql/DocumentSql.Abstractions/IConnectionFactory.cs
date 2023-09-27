using System;
using System.Data.Common;

namespace DocumentSql
{
    public interface IConnectionFactory
    {
        DbConnection CreateConnection();
        Type DbConnectionType { get; }
    }
}

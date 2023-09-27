using System.Data;

namespace DocumentSql.Schema
{
    public interface IAlterColumnCommand : IColumnCommand
    {
        IAlterColumnCommand WithType(DbType dbType, int? length);
        IAlterColumnCommand WithType(DbType dbType, byte precision, byte scale);
    }
}

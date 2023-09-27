using System;

namespace DocumentSql.Storage
{
    public interface IIdentityEntity
    {
        int Id { get; set; }
        object Entity { get; set; }
        Type EntityType { get; set; }
    }
}

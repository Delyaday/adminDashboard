using System;

namespace DocumentSql.Storage
{
    public class DocumentIdentity : IIdentityEntity
    {
        public int Id { get; set; }
        public object Entity { get; set; }
        public Type EntityType { get; set; }

        public DocumentIdentity(int id, object entity)
        {
            Id = id;
            Entity = entity;
            EntityType = entity.GetType();
        }

        public DocumentIdentity(int id, Type type)
        {
            Id = id;
            Entity = null;
            EntityType = type;
        }
    }
}

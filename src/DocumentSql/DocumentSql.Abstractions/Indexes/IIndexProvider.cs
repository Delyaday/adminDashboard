using System;

namespace DocumentSql.Indexes
{
    public interface IIndexProvider
    {
        void Describe(IDescriptor context);
        Type ForType();
        string CollectionName { get; set; }
    }
}

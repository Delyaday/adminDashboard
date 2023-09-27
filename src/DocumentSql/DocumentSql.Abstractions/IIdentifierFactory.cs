using System;

namespace DocumentSql
{
    public interface IIdentifierFactory
    {
        IIdAccessor<T> CreateAccessor<T>(Type tContainer, string name);
    }
}

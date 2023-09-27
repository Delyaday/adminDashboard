using System;
using System.Collections.Generic;

namespace DocumentSql.Indexes
{
    public interface IDescriptor
    {
        IEnumerable<IndexDescriptor> Describe(params Type[] types);
        bool IsCompatibleWith(Type target);
    }
}

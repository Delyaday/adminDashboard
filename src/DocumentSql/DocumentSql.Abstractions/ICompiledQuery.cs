using System;
using System.Linq.Expressions;

namespace DocumentSql
{
    public interface ICompiledQuery<T> where T : class
    {
        Expression<Func<IQuery<T>, IQuery<T>>> Query();
    }
}

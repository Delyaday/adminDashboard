using DocumentSql.Indexes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DocumentSql
{
    public interface IQuery
    {
        IQuery<T> For<T>(bool filterType = true) where T : class;
        IQueryIndex<T> ForIndex<T>() where T : class, IIndex;
        IQuery<object> Any();
    }

    public interface IQuery<T> where T : class
    {
        IQuery<T, TIndex> With<TIndex>() where TIndex : class, IIndex;
        IQuery<T, TIndex> With<TIndex>(Expression<Func<TIndex, bool>> predicate) where TIndex : class, IIndex;
        IQuery<T> Skip(int count);
        IQuery<T> Take(int count);
        Task<T> FirstOrDefaultAsync();
        Task<IEnumerable<T>> ListAsync();
        IAsyncEnumerable<T> ToAsyncEnumerable();
        Task<int> CountAsync();
    }

    public interface IQueryIndex<T> where T : IIndex
    {
        IQueryIndex<TIndex> With<TIndex>() where TIndex : class, IIndex;
        IQueryIndex<TIndex> With<TIndex>(Expression<Func<TIndex, bool>> predicate) where TIndex : class, IIndex;
        IQueryIndex<T> Where(string sql);
        IQueryIndex<T> Where(Func<ISqlDialect, string> sql);
        IQueryIndex<T> Where(Expression<Func<T, bool>> predicate);
        IQueryIndex<T> WithParameter(string name, object value);
        IQueryIndex<T> OrderBy(Expression<Func<T, object>> keySelector);
        IQueryIndex<T> OrderByDescending(Expression<Func<T, object>> keySelector);
        IQueryIndex<T> ThenBy(Expression<Func<T, object>> keySelector);
        IQueryIndex<T> ThenByDescending(Expression<Func<T, object>> keySelector);
        IQueryIndex<T> Skip(int count);
        IQueryIndex<T> Take(int count);
        Task<T> FirstOrDefaultAsync();
        Task<IEnumerable<T>> ListAsync();
        IAsyncEnumerable<T> ToAsyncEnumerable();
        Task<int> CountAsync();
    }

    public interface IQuery<T, TIndex> : IQuery<T>
        where T : class
        where TIndex : IIndex
    {
        IQuery<T, TIndex> Where(string sql);
        IQuery<T, TIndex> Where(Func<ISqlDialect, string> sql);
        IQuery<T, TIndex> WithParameter(string name, object value);
        IQuery<T, TIndex> Where(Expression<Func<TIndex, bool>> predicate);
        IQuery<T, TIndex> OrderBy(Expression<Func<TIndex, object>> keySelector);
        IQuery<T, TIndex> OrderBy(string sql);
        IQuery<T, TIndex> OrderByDescending(Expression<Func<TIndex, object>> keySelector);
        IQuery<T, TIndex> OrderByDescending(string sql);
        IQuery<T, TIndex> ThenBy(Expression<Func<TIndex, object>> keySelector);
        IQuery<T, TIndex> ThenBy(string sql);
        IQuery<T, TIndex> ThenByDescending(Expression<Func<TIndex, object>> keySelector);
        IQuery<T, TIndex> ThenByDescending(string sql);
    }
}

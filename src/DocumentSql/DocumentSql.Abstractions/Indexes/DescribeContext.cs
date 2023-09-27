using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DocumentSql.Indexes
{
    public class DescribeContext<T> : IDescriptor
    {
        private readonly Dictionary<Type, List<IDescribeFor>> _describes = new Dictionary<Type, List<IDescribeFor>>();

        public IEnumerable<IndexDescriptor> Describe(params Type[] types)
        {
            return _describes
                    .Where(f => types == null || types.Length == 0 || types.Contains(f.Key))
                    .SelectMany(f => f.Value)
                    .Select(f => new IndexDescriptor
                    {
                        Type = f.IndexType,
                        Map = f.GetMap(),
                        Reduce = f.GetReduce(),
                        Delete = f.GetDelete(),
                        GroupKey = f.GroupProperty,
                        IndexType = f.IndexType
                    });
        }

        public bool IsCompatibleWith(Type target)
        {
            return typeof(T).GetTypeInfo().IsAssignableFrom(target.GetTypeInfo());
        }

        public IMapFor<T, TIndex> For<TIndex>() where TIndex : IIndex
        {
            return For<TIndex, object>();
        }

        public IMapFor<T, TIndex> For<TIndex, TKey>() where TIndex : IIndex
        {
            if (!_describes.TryGetValue(typeof(T), out var descriptors))
            {
                descriptors = _describes[typeof(T)] = new List<IDescribeFor>();
            }

            var describeFor = new IndexDescriptor<T, TIndex, TKey>();
            descriptors.Add(describeFor);
            return describeFor;
        }
    }
}

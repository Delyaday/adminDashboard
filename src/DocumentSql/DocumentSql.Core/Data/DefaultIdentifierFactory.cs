using System;
using System.Reflection;

namespace DocumentSql.Data
{
    public class DefaultIdentifierFactory : IIdentifierFactory
    {
        public IIdAccessor<T> CreateAccessor<T>(Type tContainer, string name)
        {
            var propertyInfo = tContainer.GetTypeInfo().GetProperty(name);

            if (propertyInfo == null)
            {
                return null;
            }

            var tProperty = propertyInfo.PropertyType;

            var getType = typeof(Func<,>).MakeGenericType(new[] { tContainer, tProperty });
            var setType = typeof(Action<,>).MakeGenericType(new[] { tContainer, tProperty });

            var getter = propertyInfo.GetGetMethod().CreateDelegate(getType);
            var setter = propertyInfo.GetSetMethod(true).CreateDelegate(setType);

            var accessorType = typeof(IdAccessor<,>).MakeGenericType(tContainer, tProperty);

            return Activator.CreateInstance(accessorType, new object[] { getter, setter }) as IIdAccessor<T>;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;

namespace DocumentSql.Sql.Schema
{
    public static class SchemaUtils
    {
        private static Dictionary<Type, DbType> DbTypes = new Dictionary<Type, DbType>
        {
            { typeof(object), DbType.Binary },
            { typeof(byte[]), DbType.Binary },
            { typeof(string), DbType.String },
            { typeof(char), DbType.String },
            { typeof(bool), DbType.Boolean },
            { typeof(sbyte), DbType.SByte },
            { typeof(short), DbType.Int16 },
            { typeof(ushort), DbType.UInt16 },
            { typeof(int), DbType.Int32 },
            { typeof(uint), DbType.UInt32 },
            { typeof(long), DbType.Int64 },
            { typeof(ulong), DbType.UInt64 },
            { typeof(float), DbType.Single },
            { typeof(double), DbType.Double },
            { typeof(decimal), DbType.Decimal },
            { typeof(DateTime), DbType.DateTime },
            { typeof(DateTimeOffset), DbType.DateTime },
            { typeof(Guid), DbType.String }
        };


        public static DbType ToDbType(Type type)
        {
            if (DbTypes.TryGetValue(type, out var dbType))
            {
                return dbType;
            }

            if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var nullable = Nullable.GetUnderlyingType(type);

                if (nullable != null)
                    return ToDbType(nullable);
            }

            return DbType.Object;
        }
    }
}

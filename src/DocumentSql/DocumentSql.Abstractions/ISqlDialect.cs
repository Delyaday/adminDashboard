using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using DocumentSql.Indexes;

namespace DocumentSql
{
    public interface ISqlDialect
    {
        string Name { get; }
        string CascadeConstraintsString { get; }
        string CreateTableString { get; }
        string PrimaryKeyString { get; }
        string NullColumnString { get; }
        bool SupportsUnique { get; }
        bool PrefixIndex { get; }
        bool HasDataTypeInIdentityColumn { get; }
        bool SupportsIdentityColumns { get; }
        string IdentityColumnString { get; }
        string IdentitySelectString { get; }
        string GetTypeName(DbType dbType, int? length, byte precision, byte scale);
        string GetSqlValue(object value);
        string QuoteForTableName(string v);
        string GetDropTableString(string name);
        string GetDropIndexString(string indexName, string tableName);
        string QuoteForColumnName(string columnName);
        string InOperator(string values);
        string InSelectOperator(string query);
        string NotInOperator(string values);
        string NotInSelectOperator(string query);
        string GetDropForeignKeyConstraintString(string name);
        string GetAddForeignKeyConstraintString(string name, string[] srcColumns, string destTable, string[] destColumns, bool primaryKey);
        List<string> GetDistinctOrderBySelectString(List<string> select, List<string> orderBy);
        void Concat(StringBuilder builder, params Action<StringBuilder>[] generators);
        string DefaultValuesInsert { get; }
        void Page(ISqlBuilder sqlBuilder, string offset, string limit);
        ISqlBuilder CreateBuilder(string tablePrefix);
        string RenderMethod(string name, params string[] args);
        string QuoteForParameter(string parameterName);
        string ParameterNamePrefix { get; }
        string StatementEnd { get; }
        string NullString { get; }
        bool IsSpecialDistinctRequired { get; }
        IDbCommand ConfigureCommand(IDbCommand command);
        int InsertReturningIndexId(DbConnection connection, IIndex index, string insertSql, DbTransaction transaction);
        object GetDynamicParameters(DbConnection connection, object parameters, string tableName);
        object GetSafeIndexParameters(IIndex index);
    }
}

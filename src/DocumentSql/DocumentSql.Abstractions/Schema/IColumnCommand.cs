using System.Data;

namespace DocumentSql.Schema
{
    public interface IColumnCommand : ITableCommand
    {
        string ColumnName { get; }

        byte Scale { get; }

        byte Precision { get; }

        DbType DbType { get; }

        object Default { get; }

        int? Length { get; }

        IColumnCommand WithDefault(object @default);
        IColumnCommand WithLength(int? length);
        IColumnCommand Unlimited();
    }
}

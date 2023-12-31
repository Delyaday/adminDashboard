﻿namespace DocumentSql.Schema
{
    public interface IAddIndexCommand : ITableCommand
    {
        string IndexName { get; set; }
        string[] ColumnNames { get; }
    }
}

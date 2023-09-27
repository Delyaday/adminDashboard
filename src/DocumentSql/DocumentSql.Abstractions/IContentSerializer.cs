using System;

namespace DocumentSql
{
    public interface IContentSerializer
    {
        string Serialize(object item);
        object Deserialize(string content, Type type);
        dynamic DeserializeDynamic(string content);
    }
}

using System;

namespace DocumentSql
{
    public class SimplifiedTypeName : Attribute
    {
        public string Name { get; set; }

        public SimplifiedTypeName(string name)
        {
            Name = name;
        }
    }
}

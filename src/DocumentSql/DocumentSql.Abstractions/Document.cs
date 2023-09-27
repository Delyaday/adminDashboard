using System;

namespace DocumentSql
{
    public class Document : IEquatable<Document>
    {
        public int Id { get; set; }

        public string Type { get; set; }

        public string Content { get; set; }

        public long Version { get; set; }

        public bool Equals(Document other)
        {
            if (this == null && other == null)
            {
                return true;
            }
            if (this == null || other == null)
            {
                return false;
            }
            return Id == other.Id && Type == other.Type && Content == other.Content && Version == other.Version;
        }

        public override int GetHashCode()
        {
            var hashCode = 13;
            hashCode = (hashCode * 397) ^ Id;
            hashCode = (hashCode * 397) ^ (!string.IsNullOrEmpty(Type) ? Type.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (!string.IsNullOrEmpty(Content) ? Content.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (int)Version;

            return hashCode;
        }
    }
}

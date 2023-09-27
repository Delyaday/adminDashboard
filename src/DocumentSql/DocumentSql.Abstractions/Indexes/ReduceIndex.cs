using System.Collections.Generic;

namespace DocumentSql.Indexes
{
    public class ReduceIndex : IIndex
    {
        private List<Document> RemovedDocuments = new List<Document>();

        private List<Document> Documents { get; set; }

        public int Id { get; set; }

        public ReduceIndex()
        {
            Documents = new List<Document>();
        }

        void IIndex.AddDocument(Document document)
        {
            Documents.Add(document);
        }

        void IIndex.RemoveDocument(Document document)
        {
            Documents.Remove(document);
            RemovedDocuments.Add(document);
        }

        IEnumerable<Document> IIndex.GetAddedDocuments()
        {
            return Documents;
        }

        IEnumerable<Document> IIndex.GetRemovedDocuments()
        {
            return RemovedDocuments;
        }
    }
}

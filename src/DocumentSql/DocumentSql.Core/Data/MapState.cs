using DocumentSql.Indexes;
using System.Collections.Generic;

namespace DocumentSql.Data
{
    public class MapState
    {
        public IIndex Map { get; set; }
        public MapStates State { get; set; }
        public List<Document> RemovedDocuments { get; }
        public List<Document> AddedDocuments { get; }

        public MapState(IIndex map, MapStates state)
        {
            Map = map;
            State = state;
            RemovedDocuments = new List<Document>();
            AddedDocuments = new List<Document>();
        }
    }

    public enum MapStates
    {
        New,
        Update,
        Delete
    }
}

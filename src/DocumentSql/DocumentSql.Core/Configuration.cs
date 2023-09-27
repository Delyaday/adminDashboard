using DocumentSql.Data;
using DocumentSql.Serialization;
using DocumentSql.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;

namespace DocumentSql
{
    public class Configuration : IConfiguration
    {
        public Configuration()
        {
            IdentifierFactory = new DefaultIdentifierFactory();
            ContentSerializer = new JsonContentSerializer();
            IsolationLevel = IsolationLevel.ReadCommitted;
            TablePrefix = "";
            SessionPoolSize = 16;
            IdGenerator = new DefaultIdGenerator();
            Logger = NullLogger.Instance;
            QueryGatingEnabled = true;
            ConcurrentTypes = new HashSet<Type>();
        }

        public IIdentifierFactory IdentifierFactory { get; set; }
        public IsolationLevel IsolationLevel { get; set; }
        public IConnectionFactory ConnectionFactory { get; set; }
        public IContentSerializer ContentSerializer { get; set; }
        public string TablePrefix { get; set; }
        public int SessionPoolSize { get; set; }
        public bool QueryGatingEnabled { get; set; }
        public ILogger Logger { get; set; }
        public IIdGenerator IdGenerator { get; set; }
        public HashSet<Type> ConcurrentTypes { get; }
    }
}

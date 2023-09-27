using Dapper;

using DocumentSql.Collections;
using DocumentSql.Commands;
using DocumentSql.Data;
using DocumentSql.Indexes;
using DocumentSql.Serialization;
using DocumentSql.Services;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DocumentSql
{
    public class Session : ISession
    {
        private DbTransaction _transaction;
        private DbConnection _connection;
        private ISqlDialect _dialect;
        private ILogger _logger;

        private readonly IdentityMap _identityMap = new IdentityMap();
        private readonly List<IIndexCommand> _commands = new List<IIndexCommand>();
        private readonly Dictionary<IndexDescriptor, List<MapState>> _maps = new Dictionary<IndexDescriptor, List<MapState>>();
        private readonly HashSet<object> _saved = new HashSet<object>();
        private readonly HashSet<object> _updated = new HashSet<object>();
        private readonly HashSet<object> _deleted = new HashSet<object>();
        private readonly HashSet<int> _concurrent = new HashSet<int>();
        protected readonly Dictionary<string, IEnumerable<IndexDescriptor>> _descriptors = new Dictionary<string, IEnumerable<IndexDescriptor>>();
        internal readonly Store _store;
        private volatile bool _disposed;
        private bool _flushing;
        private IsolationLevel _isolationLevel;
        protected bool _cancel;
        protected List<IIndexProvider> _indexes;
        protected string _tablePrefix;

        public IStore Store => _store;

        public Session(Store store, IsolationLevel isolationLevel)
        {
            _store = store;
            _isolationLevel = isolationLevel;
            _tablePrefix = _store.Configuration.TablePrefix;
            _dialect = store.Dialect;
            _logger = store.Configuration.Logger;
        }

        public ISession RegisterIndexes(params IIndexProvider[] indexProviders)
        {
            foreach (var indexProvider in indexProviders)
            {
                if (indexProvider.CollectionName == null)
                {
                    indexProvider.CollectionName = CollectionHelper.Current.GetSafeName();
                }
            }
            if (_indexes == null)
            {
                _indexes = new List<IIndexProvider>();
            }
            _indexes.AddRange(indexProviders);
            return this;
        }

        public void Save(object entity, bool checkConcurrency = false)
        {
            CheckDisposed();

            if (_saved.Contains(entity) || _updated.Contains(entity))
            {
                return;
            }

            if (_identityMap.TryGetDocumentId(entity, out var id))
            {
                _updated.Add(entity);

                if (checkConcurrency || _store.Configuration.ConcurrentTypes.Contains(entity.GetType()))
                {
                    _concurrent.Add(id);
                }

                return;
            }

            var accessor = _store.GetIdAccessor(entity.GetType(), "Id");
            if (accessor != null)
            {
                id = accessor.Get(entity);

                if (id > 0)
                {
                    _identityMap.AddEntity(id, entity);
                    _updated.Add(entity);

                    if (checkConcurrency || _store.Configuration.ConcurrentTypes.Contains(entity.GetType()))
                    {
                        _concurrent.Add(id);
                    }

                    return;
                }
            }

            var collection = CollectionHelper.Current.GetSafeName();
            id = _store.GetNextId(collection);
            _identityMap.AddEntity(id, entity);

            accessor?.Set(entity, id);

            _saved.Add(entity);
        }

        private async Task SaveEntityAsync(object entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("obj");
            }

            if (entity is Document document)
            {
                throw new ArgumentException("A document should not be saved explicitely");
            }

            if (entity is IIndex index)
            {
                throw new ArgumentException("An index should not be saved explicitely");
            }

            var doc = new Document
            {
                Type = Store.TypeNames[entity.GetType()]
            };

            if (!_identityMap.TryGetDocumentId(entity, out var id))
            {
                throw new InvalidOperationException("The object to save was not found in identity map.");
            }

            doc.Id = id;

            await DemandAsync();

            doc.Content = Store.Configuration.ContentSerializer.Serialize(entity);
            doc.Version = 1;

            await new CreateDocumentCommand(doc, _tablePrefix).ExecuteAsync(_connection, _transaction, _dialect, _logger);

            _identityMap.AddDocument(doc);

            await MapNew(doc, entity);
        }

        public void Detach(object entity)
        {
            CheckDisposed();

            _saved.Remove(entity);
            _updated.Remove(entity);
            _deleted.Remove(entity);

            if (_identityMap.TryGetDocumentId(entity, out var id))
            {
                _identityMap.Remove(id, entity);
            }
        }

        private async Task UpdateEntityAsync(object entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("obj");
            }

            var index = entity as IIndex;

            if (entity is Document)
            {
                throw new ArgumentException("A document should not be saved explicitely");
            }

            if (index != null)
            {
                throw new ArgumentException("An index should not be saved explicitely");
            }

            if (!_identityMap.TryGetDocumentId(entity, out var id))
            {
                throw new InvalidOperationException("The object to update was not found in identity map.");
            }

            if (!_identityMap.TryGetDocument(id, out var oldDoc))
            {
                oldDoc = await GetDocumentByIdAsync(id);

                if (oldDoc == null)
                {
                    throw new InvalidOperationException("Incorrect attempt to update an object that doesn't exist. Ensure a new object was not saved with an identifier value.");
                }
            }

            long version = -1;

            if (_concurrent.Contains(id))
            {
                version = oldDoc.Version;
            }

            var oldObj = Store.Configuration.ContentSerializer.Deserialize(oldDoc.Content, entity.GetType());

            await MapDeleted(oldDoc, oldObj);

            await MapNew(oldDoc, entity);

            await DemandAsync();

            oldDoc.Content = Store.Configuration.ContentSerializer.Serialize(entity);

            if (version > -1)
                oldDoc.Version += 1;

            await new UpdateDocumentCommand(oldDoc, Store.Configuration.TablePrefix, version).ExecuteAsync(_connection, _transaction, _dialect, _logger);

            _concurrent.Remove(id);
        }

        public bool Import(object entity, int id = 0)
        {
            CheckDisposed();

            if (_saved.Contains(entity) || _updated.Contains(entity))
            {
                return false;
            }

            var doc = new Document
            {
                Type = Store.TypeNames[entity.GetType()],
                Content = Store.Configuration.ContentSerializer.Serialize(entity),
                Version = 0
            };

            if (id != 0)
            {
                _identityMap.AddEntity(id, entity);
                _updated.Add(entity);

                doc.Id = id;
                _identityMap.AddDocument(doc);

                return true;
            }
            else
            {
                var accessor = _store.GetIdAccessor(entity.GetType(), "Id");
                if (accessor != null)
                {
                    id = accessor.Get(entity);

                    if (id > 0)
                    {
                        _identityMap.AddEntity(id, entity);
                        _updated.Add(entity);

                        doc.Id = id;
                        _identityMap.AddDocument(doc);

                        return true;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Invalid 'Id' value: {id}");
                    }
                }
                else
                {
                    throw new InvalidOperationException("Objects without an 'Id' property can't be imported if no 'id' argument is provided.");
                }
            }
        }

        private async Task<Document> GetDocumentByIdAsync(int id)
        {
            await DemandAsync();

            var documentTable = CollectionHelper.Current.GetPrefixedName(DocumentSql.Store.DocumentTable);

            var command = "select * from " + _dialect.QuoteForTableName(_tablePrefix + documentTable) + " where " + _dialect.QuoteForColumnName("Id") + " = " + _dialect.QuoteForParameter("Id");
            var key = new WorkerQueryKey(nameof(GetDocumentByIdAsync), new[] { id });

            try
            {
                var result = await _store.ProduceAsync(key, (args) =>
                {
                    var localStore = (Store)args[0];
                    var localConnection = (DbConnection)args[1];
                    var localTransaction = (DbTransaction)args[2];
                    var localCommand = (string)args[3];
                    var localParameters = (object)args[4];

                    localStore.Configuration.Logger.LogTrace(localCommand);
                    return localConnection.QueryAsync<Document>(localCommand, localParameters, localTransaction);
                },
                _store,
                _connection,
                _transaction,
                command,
                new { Id = id });

                return result.FirstOrDefault();
            }
            catch
            {
                Cancel();

                throw;
            }
        }

        public void Delete(object entity)
        {
            CheckDisposed();

            _deleted.Add(entity);
        }

        private async Task DeleteEntityAsync(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            if (obj is IIndex)
            {
                throw new ArgumentException("Can't call DeleteEntity on an Index");
            }

            if (!_identityMap.TryGetDocumentId(obj, out var id))
            {
                var accessor = _store.GetIdAccessor(obj.GetType(), "Id");
                if (accessor == null)
                {
                    throw new InvalidOperationException("Could not delete object as it doesn't have an Id property");
                }

                id = accessor.Get(obj);
            }

            var doc = await GetDocumentByIdAsync(id);

            if (doc != null)
            {
                _identityMap.Remove(id, obj);

                await MapDeleted(doc, obj);

                _commands.Add(new DeleteDocumentCommand(doc, _tablePrefix));
            }
        }

        public async Task<IEnumerable<T>> GetAsync<T>(int[] ids) where T : class
        {
            if (ids == null || !ids.Any())
            {
                return Enumerable.Empty<T>();
            }

            CheckDisposed();

            await FlushAsync();

            await DemandAsync();

            var documentTable = CollectionHelper.Current.GetPrefixedName(DocumentSql.Store.DocumentTable);
            var command = "select * from " + _dialect.QuoteForTableName(_tablePrefix + documentTable) + " where " + _dialect.QuoteForColumnName("Id") + " " + _dialect.InOperator(_dialect.QuoteForParameter("Ids"));

            var key = new WorkerQueryKey(nameof(GetAsync), ids);
            try
            {
                var documents = await _store.ProduceAsync(key, (args) =>
                {
                    var localConnection = (DbConnection)args[0];
                    var localTransaction = (DbTransaction)args[1];
                    var localCommand = (string)args[2];
                    var localParamters = args[3];

                    return localConnection.QueryAsync<Document>(localCommand, localParamters, localTransaction);
                },
                _connection,
                _transaction,
                command,
                new { Ids = ids });

                return Get<T>(documents.OrderBy(d => Array.IndexOf(ids, d.Id)).ToArray());
            }
            catch
            {
                Cancel();

                throw;
            }
        }

        public IEnumerable<T> Get<T>(IList<Document> documents) where T : class
        {
            if (documents == null || !documents.Any())
            {
                return Enumerable.Empty<T>();
            }

            var result = new List<T>();

            var accessor = _store.GetIdAccessor(typeof(T), "Id");
            var typeName = Store.TypeNames[typeof(T)];

            foreach (var document in documents)
            {
                if (typeof(T) != typeof(object) && !String.Equals(typeName, document.Type, StringComparison.Ordinal))
                {
                    continue;
                }

                if (_identityMap.TryGetEntityById(document.Id, out object entity))
                {
                    result.Add((T)entity);
                }
                else
                {
                    T item;

                    if (typeof(T) == typeof(object))
                    {
                        var itemType = Store.TypeNames[document.Type];
                        accessor = _store.GetIdAccessor(itemType, "Id");

                        item = (T)Store.Configuration.ContentSerializer.Deserialize(document.Content, itemType);
                    }
                    else
                    {
                        item = (T)Store.Configuration.ContentSerializer.Deserialize(document.Content, typeof(T));
                    }

                    accessor?.Set(item, document.Id);

                    _identityMap.AddEntity(document.Id, item);
                    _identityMap.AddDocument(document);

                    result.Add(item);
                }
            }

            return result;
        }

        public IQuery Query()
        {
            return new DefaultQuery(_connection, _transaction, this, _tablePrefix);
        }

        public IQuery<T> ExecuteQuery<T>(ICompiledQuery<T> compiledQuery) where T : class
        {
            if (compiledQuery == null)
            {
                throw new ArgumentNullException(nameof(compiledQuery));
            }

            var compiledQueryType = compiledQuery.GetType();

            if (!_store.CompiledQueries.TryGetValue(compiledQueryType, out var queryState))
            {
                var localQuery = ((IQuery)new DefaultQuery(_connection, _transaction, this, _tablePrefix)).For<T>(false);
                var defaultQuery = (DefaultQuery.Query<T>)compiledQuery.Query().Compile().Invoke(localQuery);
                queryState = defaultQuery._query._queryState;

                _store.CompiledQueries = _store.CompiledQueries.SetItem(compiledQueryType, queryState);
            }

            queryState = queryState.Clone();

            IQuery newQuery = new DefaultQuery(_connection, _transaction, this, _tablePrefix, queryState, compiledQuery);
            return newQuery.For<T>(false);
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Session));
            }
        }

        ~Session()
        {
            _cancel = true;

            Dispose();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                if (!_cancel && HasWork())
                {
                    FlushAsync().GetAwaiter().GetResult();
                }
            }
            finally
            {
                _disposed = true;

                CommitTransaction();

                ReleaseSession();
            }
        }

        private void ReleaseSession()
        {
            _identityMap.Clear();
            _descriptors.Clear();
            _indexes?.Clear();

            _store.ReleaseSession(this);
        }

        private void ReleaseTransaction()
        {
            _updated.Clear();
            _concurrent.Clear();
            _saved.Clear();
            _deleted.Clear();
            _commands.Clear();
            _maps.Clear();

            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        private void ReleaseConnection()
        {
            ReleaseTransaction();

            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }

        internal void StartLease(IsolationLevel isolationLevel)
        {
            _disposed = false;
            _cancel = false;
            _isolationLevel = isolationLevel;
        }

        public async Task FlushAsync()
        {
            if (!HasWork())
            {
                return;
            }

            if (_flushing)
            {
                return;
            }

            _flushing = true;


            CheckDisposed();

            try
            {
                // saving all updated entities
                foreach (var obj in _updated)
                {
                    if (!_deleted.Contains(obj))
                    {
                        await UpdateEntityAsync(obj);
                    }
                }

                // saving all pending entities
                foreach (var obj in _saved)
                {
                    await SaveEntityAsync(obj);
                }

                // deleting all pending entities
                foreach (var obj in _deleted)
                {
                    await DeleteEntityAsync(obj);
                }

                // compute all reduce indexes
                await ReduceAsync();

                await DemandAsync();

                foreach (var command in _commands.OrderBy(x => x.ExecutionOrder))
                {
                    await command.ExecuteAsync(_connection, _transaction, _dialect, _logger);
                }
            }
            catch
            {
                Cancel();

                throw;
            }
            finally
            {
                _updated.Clear();
                _saved.Clear();
                _concurrent.Clear();
                _deleted.Clear();
                _commands.Clear();
                _maps.Clear();
                _flushing = false;
            }
        }

        public async Task CommitAsync()
        {
            try
            {
                if (!_cancel)
                {
                    await FlushAsync();
                }
            }
            finally
            {
                CommitTransaction();
            }
        }

        private void CommitTransaction()
        {
            try
            {
                if (!_cancel)
                {
                    _transaction?.Commit();
                }
                else
                {
                    _transaction?.Rollback();
                }
            }
            finally
            {
                ReleaseConnection();
            }
        }

        public bool HasWork()
        {
            return
                _saved.Count != 0 ||
                _updated.Count != 0 ||
                _deleted.Count != 0
                ;
        }

        private async Task ReduceAsync()
        {
            foreach (var descriptor in _maps.Keys)
            {
                if (descriptor.Reduce == null)
                    continue;

                if (descriptor.GroupKey == null)
                {
                    throw new InvalidOperationException("A map/reduce index must declare at least one property with a GroupKey attribut: " + descriptor.Type.FullName);
                }

                var descriptorGroup = GetGroupingMethod(descriptor);

                var allKeysForDescriptor = _maps[descriptor].Select(f => f.Map).Select(descriptorGroup).Distinct().ToArray();

                foreach (var currentKey in allKeysForDescriptor)
                {
                    var newMapsGroup = _maps[descriptor]
                        .Where(f => f.State == MapStates.New)
                        .Select(f => f.Map)
                        .Where(f => descriptorGroup(f).Equals(currentKey))
                        .ToArray();

                    var deletedMapsGroup = _maps[descriptor]
                        .Where(x => x.State == MapStates.Delete)
                        .Select(x => x.Map)
                        .Where(x => descriptorGroup(x).Equals(currentKey))
                        .ToArray();

                    var updatedMapsGroup = _maps[descriptor]
                        .Where(x => x.State == MapStates.Update)
                        .Select(x => x.Map)
                        .Where(x => descriptorGroup(x).Equals(currentKey))
                        .ToArray();

                    IIndex index = null;

                    if (newMapsGroup.Any())
                    {
                        index = descriptor.Reduce(newMapsGroup.GroupBy(descriptorGroup).First());

                        if (index == null)
                        {
                            throw new InvalidOperationException("The reduction on a grouped set should have resulted in a unique result");
                        }
                    }

                    var dbIndex = await ReduceForAsync(descriptor, currentKey);

                    if (dbIndex != null && index != null)
                    {
                        var reductions = new[] { dbIndex, index };

                        var groupedReductions = reductions.GroupBy(descriptorGroup).SingleOrDefault();

                        if (groupedReductions == null)
                        {
                            throw new InvalidOperationException("The grouping on the db and in memory set should have resulted in a unique result");
                        }

                        index = descriptor.Reduce(groupedReductions);

                        if (index == null)
                        {
                            throw new InvalidOperationException("The redction on a grouped set should have resulted in a unique result");
                        }
                    }
                    else if (dbIndex != null)
                    {
                        index = dbIndex;
                    }

                    if (index != null)
                    {
                        if (deletedMapsGroup.Any())
                        {
                            index = descriptor.Delete(index, deletedMapsGroup.GroupBy(descriptorGroup).First());
                        }

                        if (updatedMapsGroup.Any())
                        {
                            index = descriptor.Update(index, updatedMapsGroup.GroupBy(descriptorGroup).First());
                        }
                    }

                    var deletedDocumentIds = deletedMapsGroup.SelectMany(f => f.GetRemovedDocuments().Select(d => d.Id)).ToArray();
                    var addedDocumentIds = newMapsGroup.SelectMany(f => f.GetAddedDocuments().Select(d => d.Id)).ToArray();

                    if (dbIndex != null)
                    {
                        if (index == null)
                        {
                            _commands.Add(new DeleteReduceIndexCommand(dbIndex, _tablePrefix));
                        }
                        else
                        {
                            index.Id = dbIndex.Id;

                            var common = addedDocumentIds.Intersect(deletedDocumentIds).ToArray();
                            addedDocumentIds = addedDocumentIds.Where(f => !common.Contains(f)).ToArray();
                            deletedDocumentIds = deletedDocumentIds.Where(f => !common.Contains(f)).ToArray();

                            _commands.Add(new UpdateIndexCommand(index, addedDocumentIds, deletedDocumentIds, _tablePrefix));
                        }
                    }
                    else
                    {
                        if (index != null)
                        {
                            _commands.Add(new CreateIndexCommand(index, addedDocumentIds, _tablePrefix));
                        }
                    }
                }
            }
        }

        private async Task<ReduceIndex> ReduceForAsync(IndexDescriptor descriptor, object currentKey)
        {
            await DemandAsync();

            var name = _tablePrefix + descriptor.IndexType.Name;
            var sql = "select * from " + _dialect.QuoteForTableName(name) + " where " + _dialect.QuoteForColumnName(descriptor.GroupKey.Name) + " = @currentKey";

            var index = await _connection.QueryAsync(descriptor.IndexType, sql, new { currentKey }, _transaction);
            return index.FirstOrDefault() as ReduceIndex;
        }

        private Func<IIndex, object> GetGroupingMethod(IndexDescriptor descriptor)
        {
            if (!_store.GroupMethods.TryGetValue(descriptor.Type, out var result))
            {
                // IIndex i => i
                var instance = Expression.Parameter(typeof(IIndex), "i");
                // i => ((TIndex)i)
                var convertInstance = Expression.Convert(instance, descriptor.GroupKey.DeclaringType);
                // i => ((TIndex)i).{Property}
                var property = Expression.Property(convertInstance, descriptor.GroupKey);
                // i => (object)(((TIndex)i).{Property})
                var convert = Expression.Convert(property, typeof(object));

                result = Expression.Lambda<Func<IIndex, object>>(convert, instance).Compile();

                _store.GroupMethods = _store.GroupMethods.SetItem(descriptor.Type, result);
            }

            return result;
        }

        private IEnumerable<IndexDescriptor> GetDescriptors(Type t)
        {
            var cacheKey = t.FullName + ":" + CollectionHelper.Current.GetSafeName();
            if (!_descriptors.TryGetValue(cacheKey, out var typedDescriptors))
            {
                typedDescriptors = _store.Describe(t);
                if (_indexes != null)
                {
                    var collection = CollectionHelper.Current.GetSafeName();
                    typedDescriptors = typedDescriptors.Union(_store.CreateDescriptors(t, collection, _indexes)).ToArray();
                }
                _descriptors.Add(cacheKey, typedDescriptors);
            }
            return typedDescriptors;
        }

        private async Task MapNew(Document document, object obj)
        {
            foreach (var descriptor in GetDescriptors(obj.GetType()))
            {
                var mapped = await descriptor.Map(obj);

                foreach (var index in mapped)
                {
                    if (index == null)
                        continue;

                    index.AddDocument(document);

                    if (descriptor.Reduce == null)
                    {
                        if (index.Id == 0)
                        {
                            _commands.Add(new CreateIndexCommand(index, Enumerable.Empty<int>(), _tablePrefix));
                        }
                        else
                        {
                            _commands.Add(new UpdateIndexCommand(index, Enumerable.Empty<int>(), Enumerable.Empty<int>(), _tablePrefix));
                        }
                    }
                    else
                    {
                        if (!_maps.TryGetValue(descriptor, out var listmap))
                        {
                            _maps.Add(descriptor, listmap = new List<MapState>());
                        }

                        listmap.Add(new MapState(index, MapStates.New));
                    }
                }
            }
        }

        private async Task MapDeleted(Document document, object obj)
        {
            foreach (var descriptor in _store.Describe(obj.GetType()))
            {
                if (descriptor.Reduce == null || descriptor.Delete == null)
                {
                    _commands.Add(new DeleteMapIndexCommand(descriptor.IndexType, document.Id, _tablePrefix, _dialect));
                }
                else
                {
                    var mapped = await descriptor.Map(obj);
                    foreach (var index in mapped)
                    {
                        if (!_maps.TryGetValue(descriptor, out var listmap))
                        {
                            _maps.Add(descriptor, listmap = new List<MapState>());
                        }

                        listmap.Add(new MapState(index, MapStates.Delete));
                        index.RemoveDocument(document);
                    }
                }
            }
        }

        public async Task<DbTransaction> DemandAsync()
        {
            CheckDisposed();

            if (_transaction == null)
            {
                if (_connection == null)
                {
                    _connection = _store.Configuration.ConnectionFactory.CreateConnection() as DbConnection;

                    if (_connection == null)
                    {
                        throw new InvalidOperationException("The connection couldn't be covnerted to DbConnection");
                    }
                }

                if (_connection.State == ConnectionState.Closed)
                {
                    await _connection.OpenAsync();
                }

                _transaction = _connection.BeginTransaction(_isolationLevel);

                _cancel = false;
            }

            return _transaction;
        }

        public void Cancel()
        {
            CheckDisposed();

            _cancel = true;

            ReleaseTransaction();
        }
    }
}

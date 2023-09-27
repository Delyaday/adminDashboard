using DocumentSql;
using DocumentSql.Sql;

using Foundation.Data.Migrations.Records;

using Microsoft.Extensions.Logging;

using System.Reflection;

namespace Foundation.Data.Migrations
{
    public interface IDataMigrationManager
    {
        Task<IEnumerable<string>> GetMigrationsThatNeedUpdateAsync();

        Task UpdateAllAsync();

        Task UpdateAsync(string migrationClassName);

        Task UpdateAsync(IEnumerable<string> migrationClassNames);

        Task Uninstall(string migrationClassName);
    }

    public class DataMigrationManager : IDataMigrationManager
    {
        private readonly IEnumerable<IDataMigration> _dataMigrations;
        private readonly ISession _session;
        private readonly IStore _store;
        private readonly ILogger _logger;

        private readonly List<string> _processedMigrations;
        private DataMigrationRecord _dataMigrationRecord;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataMigrations"></param>
        /// <param name="session"></param>
        /// <param name="store"></param>
        /// <param name="logger"></param>
        public DataMigrationManager(IEnumerable<IDataMigration> dataMigrations, ISession session, IStore store, ILogger<DataMigrationManager> logger)
        {
            _dataMigrations=dataMigrations;
            _session=session;
            _store=store;
            _logger=logger;

            _processedMigrations=new List<string>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<DataMigrationRecord> GetDataMigrationRecordAsync()
        {
            if (_dataMigrationRecord == null)
            {
                _dataMigrationRecord = await _session.Query<DataMigrationRecord>().FirstOrDefaultAsync();

                if (_dataMigrationRecord == null)
                {
                    _dataMigrationRecord = new DataMigrationRecord();
                    _session.Save(_dataMigrationRecord);
                }
            }

            return _dataMigrationRecord;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetMigrationsThatNeedUpdateAsync()
        {
            var currentVersions = (await GetDataMigrationRecordAsync()).DataMigrations.ToDictionary(r => r.DataMigrationClass);

            var outOfDateMigrations = _dataMigrations.Where(dataMigration =>
            {
                if (currentVersions.TryGetValue(dataMigration.GetType().FullName, out Records.DataMigration record) && record.Version.HasValue)
                {
                    return CreateUpgradeLookupTable(dataMigration).ContainsKey(record.Version.Value);
                }

                return (GetCreateMethod(dataMigration) != null);
            });

            return outOfDateMigrations.Select(m => m.GetType().FullName).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="migrationClassName"></param>
        /// <returns></returns>
        public async Task Uninstall(string migrationClassName)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Uninstalling migrations: {0}.", migrationClassName);
            }
            var migration = GetDataMigration(migrationClassName);

            var tempMigration = migration;

            var dataMigrationRecord = await GetDataMigrationRecordAsync(tempMigration);

            var uninstallMethod = GetUninstallMethod(migration);
            if (uninstallMethod != null)
            {
                uninstallMethod.Invoke(migration, new object[0]);
            }

            if (dataMigrationRecord == null)
            {
                return;
            }

            (await GetDataMigrationRecordAsync()).DataMigrations.Remove(dataMigrationRecord);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="migrationClassNames"></param>
        /// <returns></returns>
        public async Task UpdateAsync(IEnumerable<string> migrationClassNames)
        {
            foreach (var className in migrationClassNames)
            {
                if (!_processedMigrations.Contains(className))
                {
                    await UpdateAsync(className);
                }
            }
        }

        public async Task UpdateAsync(string migrationClassName)
        {
            if (_processedMigrations.Contains(migrationClassName))
            {
                return;
            }

            _processedMigrations.Add(migrationClassName);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Updating migrations: {0}", migrationClassName);
            }

            var migration = GetDataMigration(migrationClassName);

            var schemaBuilder = new SchemaBuilder(_store.Configuration, await _session.DemandAsync());
            migration.SchemaBuilder = schemaBuilder;

            var tempMigration = migration;

            var dataMigrationRecord = GetDataMigrationRecordAsync(tempMigration).Result;

            var current = 0;
            if (dataMigrationRecord != null)
            {
                current = dataMigrationRecord.Version.Value;
            }
            else
            {
                dataMigrationRecord = new Records.DataMigration { DataMigrationClass = migration.GetType().FullName };
                _dataMigrationRecord.DataMigrations.Add(dataMigrationRecord);
            }

            try
            {
                if (current == 0)
                {
                    var createMethod = GetCreateMethod(migration);
                    if (createMethod != null)
                    {
                        current = (int)createMethod.Invoke(migration, new object[0]);
                    }
                }

                var lookupTable = CreateUpgradeLookupTable(migration);

                while (lookupTable.ContainsKey(current))
                {
                    try
                    {
                        if (_logger.IsEnabled(LogLevel.Information))
                        {
                            _logger.LogInformation("Applying migration for {0} from version {1}.", migrationClassName, current);
                        }
                        current = (int)lookupTable[current].Invoke(migration, new object[0]);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(0, "An unexpected error occurred while applying migration on {0} from version {1}.", migrationClassName, current);
                        throw;
                    }
                }

                if (current == 0)
                {
                    return;
                }

                dataMigrationRecord.Version = current;
            }
            catch (Exception ex)
            {
                _logger.LogError(0, "Error while running migration version {0} for {1}.", current, migrationClassName);
                _session.Cancel();
                throw new Exception($"Error while running migration version {current} for {migrationClassName}.", ex);
            }
            finally
            {
                _session.Save(_dataMigrationRecord);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task UpdateAllAsync()
        {
            var migrationsThatNeedUpdate = await GetMigrationsThatNeedUpdateAsync();

            foreach (var className in migrationsThatNeedUpdate)
            {
                try
                {
                    await UpdateAsync(className);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Could not run migrations automatically on " + className, ex);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tempMigration"></param>
        /// <returns></returns>
        private async Task<Records.DataMigration> GetDataMigrationRecordAsync(IDataMigration tempMigration)
        {
            var dataMigrationRecord = await GetDataMigrationRecordAsync();
            return dataMigrationRecord
                .DataMigrations
                .FirstOrDefault(dm => dm.DataMigrationClass == tempMigration.GetType().FullName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="migrationClassName"></param>
        /// <returns></returns>
        private IDataMigration GetDataMigration(string migrationClassName)
        {
            return _dataMigrations.FirstOrDefault(dm => dm.GetType().FullName == migrationClassName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataMigration"></param>
        /// <returns></returns>
        private Dictionary<int, MethodInfo> CreateUpgradeLookupTable(IDataMigration dataMigration)
        {
            return dataMigration
                .GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Select(GetUpdateMethod)
                .Where(tuple => tuple != null)
                .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mi"></param>
        /// <returns></returns>
        private Tuple<int, MethodInfo> GetUpdateMethod(MethodInfo mi)
        {
            const string UPDATE_FROM_PREFIX = "UpdateFrom";

            if (mi.Name.StartsWith(UPDATE_FROM_PREFIX))
            {
                var version = mi.Name.Substring(UPDATE_FROM_PREFIX.Length);
                if (int.TryParse(version, out var value))
                {
                    return new Tuple<int, MethodInfo>(value, mi);
                }
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataMigration"></param>
        /// <returns></returns>
        private MethodInfo GetCreateMethod(IDataMigration dataMigration)
        {
            var methodInfo = dataMigration.GetType().GetMethod("Create", BindingFlags.Public | BindingFlags.Instance);
            if (methodInfo != null && methodInfo.ReturnType == typeof(int))
            {
                return methodInfo;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataMigration"></param>
        /// <returns></returns>
        private MethodInfo GetUninstallMethod(IDataMigration dataMigration)
        {
            var methodInfo = dataMigration.GetType().GetMethod("Uninstall", BindingFlags.Public | BindingFlags.Instance);
            if (methodInfo != null && methodInfo.ReturnType == typeof(void))
            {
                return methodInfo;
            }

            return null;
        }
    }
}

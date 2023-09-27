using System;
using System.Threading.Tasks;

namespace DocumentSql
{
    public class StoreFactory
    {
        public static async Task<IStore> CreateAsync(Action<IConfiguration> configuration)
        {
            var store = new Store(configuration);
            await store.InitializeAsync();
            return store;
        }

        public static async Task<IStore> CreateAsync(IConfiguration configuration)
        {
            var store = new Store(configuration);
            await store.InitializeAsync();
            return store;
        }
    }
}

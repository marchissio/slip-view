
namespace SB.Tickets.Api.Cache
{
    public class CacheService<T> where T : ICacheItem, new()
    {
        private readonly ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim();
        private readonly Dictionary<string, Models.League> _cache = new Dictionary<string, Models.League> 
        {
            { "1", new Models.League { Id = 1, Name = "Liga Sampiona", Sport = "S", Country = "Evropa", ShortName = "LS" } },
            { "2", new Models.League { Id = 2, Name = "Engleska 1", Sport = "S", Country = "Engleska", ShortName = "EN" } }
            //TODO: Dodaj jos liga ako treba
        };

        public ICacheItem Get(string id)
        {
            if (id == null)
                return null;

            _cacheLock.EnterReadLock();
            try
            {
                if (_cache.ContainsKey(id))
                {
                    var state = _cache[id];
                    return state.GetData();
                }
            }
            finally
            {
                _cacheLock.ExitReadLock();
            }
            return null;
        }
    }
}

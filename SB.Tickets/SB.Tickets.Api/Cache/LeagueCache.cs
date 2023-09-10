using SB.Tickets.Api.Cache.Models;

namespace SB.Tickets.Api.Cache
{
    public static class LeagueCache
    {
        public static CacheService<League> Instance = new CacheService<League>();
    }
}

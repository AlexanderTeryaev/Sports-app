using DataService;
using DataService.LeagueRank;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace WebApi.Helpers
{
    public class Caching
    {
        public static T GetObjectFromCache<T>(string cacheItemName)
        {
            ObjectCache cache = MemoryCache.Default;
            var cachedObject = (T)cache[cacheItemName];
            return cachedObject;
        }

        public static void SetObjectInCache(string cacheItemName, int cacheTimeInSeconds, object value)
        {
            ObjectCache cache = MemoryCache.Default;
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(cacheTimeInSeconds);
            cache.Set(cacheItemName, value, policy);
        }

    }

    public static class CacheService
    {
        public static RankLeague CreateLeagueRankTable(int leagueId, int? seasonId = null, bool isTennisLeague = false)
        {
            var rLeague = Caching.GetObjectFromCache<RankLeague>($"{CacheKeys.LeagueRanks}_CreateLeagueRankTable_{leagueId.ToString()}_{seasonId.ToString()}_{isTennisLeague.ToString()}");
            if (rLeague == null)
            {
                LeagueRankService lrsvc = new LeagueRankService(leagueId);
                rLeague = lrsvc.CreateLeagueRankTable(seasonId, isTennisLeague);
                Caching.SetObjectInCache($"{CacheKeys.LeagueRanks}_CreateLeagueRankTable_{leagueId.ToString()}_{seasonId.ToString()}_{isTennisLeague.ToString()}", 300, rLeague);
            }
            return rLeague;
        }


        public static RankLeague CreateEmptyRankTable(int leagueId, int? seasonId = null)
        {
            var rLeague = Caching.GetObjectFromCache<RankLeague>($"{CacheKeys.LeagueRanks}_CreateEmptyRankTable_{leagueId.ToString()}_{seasonId.ToString()}");
            if (rLeague == null)
            {
                LeagueRankService lrsvc = new LeagueRankService(leagueId);
                rLeague = lrsvc.CreateEmptyRankTable(seasonId);
                Caching.SetObjectInCache($"{CacheKeys.LeagueRanks}_CreateEmptyRankTable_{leagueId.ToString()}_{seasonId.ToString()}", 300, rLeague);
            }
            return rLeague;
        }

        public static List<TennisLeagueGameForm> GetAllTennisGamesForUser(GamesRepo gRep, int id, int teamId, int seasonId, bool isHebrew)
        {
            
            var result = Caching.GetObjectFromCache<List<TennisLeagueGameForm>>($"{CacheKeys.AllTennisGamesForUser}_{id.ToString()}_{teamId.ToString()}_{seasonId.ToString()}");
            if (result == null)
            {
                result = gRep.GetAllTennisGamesForUser(id, teamId, seasonId, isHebrew);
                Caching.SetObjectInCache($"{CacheKeys.AllTennisGamesForUser}_{id.ToString()}_{teamId.ToString()}_{seasonId.ToString()}", 300, result);
            }
            return result;
        }



    }
    public static class CacheKeys
    {
        public static string LeagueRanks { get { return "_LeagueRanksKey"; } }
        public static string AllTennisGamesForUser { get { return "_AllTennisGamesForUser"; } }
    }
}
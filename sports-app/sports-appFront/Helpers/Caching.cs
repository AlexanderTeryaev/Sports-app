using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace LogLigFront.Helpers
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

        public static void ClearCacheValue(string key)
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove(key);
        }
    }
    public static class CacheKeys
    {
        public static string CompetitionRegistrationsKey { get { return "_CompetitionRegistrationsKey"; } }
        public static string DisciplineCompetitionKey { get { return "_disciplineCompetition"; } }

        public static string DisciplineCompetitionKeyLastUpdate { get { return "_disciplineCompetitionLastUpdate"; } }
        
        public static string CompetitionClubsRankingsKey { get { return "_competitionClubsRankingsKey"; } }
        public static string CompetitionKey { get { return "_competitionKey"; } }
        public static string CompetitionResultsKey { get { return "_CompetitionResultsKey"; } }
    }
 }
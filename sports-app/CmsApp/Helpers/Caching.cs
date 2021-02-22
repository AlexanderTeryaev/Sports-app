using AppModel;
using DataService;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace CmsApp.Helpers
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

        public static void DeleteAllCache()
        {
            ObjectCache cacheMemory = MemoryCache.Default;
            foreach (var cache in cacheMemory)
            {
                cacheMemory.Remove(cache.Key);
            }
            
        }

        public static string GetTeamPlayersShortKey(int unionId, int seasonId)
        {
            return $"{CacheKeys.TeamPlayersByUnionIdShort}_{unionId}_{seasonId}";
            
        }

        public static List<TeamsPlayer> GetTeamPlayersByUnionIdShort(PlayersRepo playersRepo, int unionId,int seasonId)
        {
            var cacheKey = GetTeamPlayersShortKey(unionId, seasonId);
            var players = GetObjectFromCache<List<TeamsPlayer>>(cacheKey);
            if (players == null)
            {
                players = playersRepo.GetTeamPlayersByUnionIdShort(unionId, seasonId);
                SetObjectInCache(cacheKey, 60, players);
            }
            return players;
        }

    }
    public static class CacheKeys
    {
        public static string TeamPlayersByUnionIdShort = "TeamPlayersByUnionIdShort";
    }
 }
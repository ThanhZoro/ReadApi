using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReadApi.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class RedisExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="queryKey"></param>
        /// <param name="newKey"></param>
        /// <returns></returns>
        public static async Task SaveKey(this IDistributedCache cache, string queryKey, string newKey)
        {
            List<string> keys = new List<string>();
            var listKeyCache = $"list-{queryKey}";
            string cachedJson = await cache.GetStringAsync(listKeyCache);
            if (!string.IsNullOrEmpty(cachedJson))
            {
                keys = JsonConvert.DeserializeObject<List<string>>(cachedJson);
            }
            keys.Add(newKey);
            await cache.SetStringAsync(listKeyCache, JsonConvert.SerializeObject(keys), new DistributedCacheEntryOptions() { AbsoluteExpiration = DateTime.Now.AddMinutes(30) });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="queryKey"></param>
        /// <returns></returns>
        public static async Task RemoveKeys(this IDistributedCache cache, string queryKey)
        {
            List<string> keys = new List<string>();
            var listKeyCache = $"list-{queryKey}";
            string cachedJson = await cache.GetStringAsync(listKeyCache);
            if (!string.IsNullOrEmpty(cachedJson))
            {
                keys = JsonConvert.DeserializeObject<List<string>>(cachedJson);
            }
            foreach (var item in keys)
            {
                await cache.RemoveAsync(item);
            }
        }
    }
}


using Nest;
using Newtonsoft.Json;
using ReadApi.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ReadApi.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class ElasticSearchQueryExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        public static string GetHashValue(this ElasticSearchQuery data)
        {
            var hashValue = string.Join("", MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(data))).Select(s => s.ToString("x2")));
            return hashValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string GetHashValueWithMoreParams(this ElasticSearchQuery data, dynamic[] param)
        {
            var hashValue = string.Join("", MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(data) + JsonConvert.SerializeObject(param))).Select(s => s.ToString("x2")));
            return hashValue;
        }
        
        private static readonly Object locker = new Object();
        
        private static byte[] ObjectToByteArray(Object objectToSerialize)
        {
            MemoryStream fs = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                //Here's the core functionality! One Line!
                //To be thread-safe we lock the object
                lock (locker)
                {
                    formatter.Serialize(fs, objectToSerialize);
                }
                return fs.ToArray();
            }
            catch (SerializationException se)
            {
                Console.WriteLine("Error occurred during serialization. Message: " +
                se.Message);
                return null;
            }
            finally
            {
                fs.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_esClient"></param>
        /// <param name="index"></param>
        /// <param name="type"></param>
        /// <param name="query"></param>
        /// <param name="scrollTimeout"></param>
        /// <param name="scrollSize"></param>
        /// <returns></returns>
        public static async Task<List<T>> GetAllDocuments<T>(this ElasticClient _esClient, string index, string type, QueryContainer query, string scrollTimeout = "2m", int scrollSize = 10000) where T : class
        {
            //The thing to know about scrollTimeout is that it resets after each call to the scroll so it only needs to be big enough to stay alive between calls.
            //when it expires, elastic will delete the entire scroll.
            ISearchResponse<T> initialResponse = await _esClient.SearchAsync<T>
                (scr => scr
                     .Index(index)
                     .Type(type)
                     .From(0)
                     .Take(scrollSize)
                     .Query(q => { return query; })
                     .Scroll(scrollTimeout));
            List<T> results = new List<T>();
            if (!initialResponse.IsValid || string.IsNullOrEmpty(initialResponse.ScrollId))
                throw new Exception(initialResponse.ServerError.Error.Reason);
            if (initialResponse.Documents.Any())
                results.AddRange(initialResponse.Documents);
            string scrollid = initialResponse.ScrollId;
            bool isScrollSetHasData = true;
            while (isScrollSetHasData)
            {
                ISearchResponse<T> loopingResponse = await _esClient.ScrollAsync<T>(scrollTimeout, scrollid);
                if (loopingResponse.IsValid)
                {
                    results.AddRange(loopingResponse.Documents);
                    scrollid = loopingResponse.ScrollId;
                }
                isScrollSetHasData = loopingResponse.Documents.Any();
            }
            //This would be garbage collected on it's own after scrollTimeout expired from it's last call but we'll clean up our room when we're done per best practice.
            _esClient.ClearScroll(new ClearScrollRequest(scrollid));
            return results;
        }
    }
}

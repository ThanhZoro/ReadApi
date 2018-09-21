using MongoDB.Driver;
using Nest;
using QueryFailOverEsMongo.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QueryFailOverEsMongo.Query
{
    public class QueryFailOverEsMongo<T> where T : class
    {
        public int From;
        public int Size;
        public int QueryTimeOut = 15000;
        public BoolQuery BoolQuery;
        public readonly List<ISort> EsSort = new List<ISort>();
        public readonly List<Aggregation> Aggregations = new List<Aggregation>();

        //connecttion
        public ElasticClient EsClient { get; }
        public IMongoCollection<T> Collection { get; }

        public QueryFailOverEsMongo(ElasticClient esClient, IMongoCollection<T> collection)
        {
            EsClient = esClient;
            Collection = collection;
        }

        public void SetBoolQuery(BoolQuery boolQuery, int from = 0, int size = 10)
        {
            BoolQuery = boolQuery;
            From = from;
            Size = size;
        }

        public void SetAggregation(AggregationType aggregationType, string columnName)
        {
            var aggregation = new Aggregation();
            aggregation.AggregationType = aggregationType;
            aggregation.ColumnName = columnName;
            Aggregations.Add(aggregation);
        }

        public void SetTimeOut(int timeout)
        {
            QueryTimeOut = timeout * 1000;
        }

        public void SetQueryTimeoutInSeconds(int second)
        {
            QueryTimeOut = second * 1000;
        }

        public void AddSort(string columnName, SortOrder sort)
        {
            EsSort.Add(new SortField { Field = columnName, Order = sort });
        }

        public async Task<DatasourceResult<List<T>>> GetListResultAsync()
        {
            var selector = new QueryExecute<T>(this);
            var result = await selector.ExecuteAsync();
            return result;
        }

        public async Task<T> GetOneResultAsync()
        {
            From = 0;
            Size = 1;
            var selector = new QueryExecute<T>(this);
            var result = await selector.ExecuteAsync();
            return result?.Data?.FirstOrDefault();
        }
    }
}

using QueryFailOverEsMongo.Common;
using QueryFailOverEsMongo.Elasticsearch;
using QueryFailOverEsMongo.Models;
using QueryFailOverEsMongo.MongoDb;
using Steeltoe.CircuitBreaker.Hystrix;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Threading.Tasks;

namespace QueryFailOverEsMongo.Query
{
    public class DatabaseSelector<T> : HystrixCommand<DatasourceResult<List<T>>> where T : class
    {
        private readonly QueryFailOverEsMongo<T> _query;

        public DatabaseSelector(QueryFailOverEsMongo<T> query) : base(HystrixCommandGroupKeyDefault.AsKey("DatabaseSelector"), query.QueryTimeOut)
        {
            _query = query;
        }

        protected override async Task<DatasourceResult<List<T>>> RunAsync()
        {
            string assemblyPath = new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath;
            Configuration cfg = ConfigurationManager.OpenExeConfiguration(assemblyPath);
            string isQueyMongo = cfg.AppSettings.Settings["AlwaysQueryMongo"].Value;
            int switchTimeDurations = int.Parse(cfg.AppSettings.Settings["SwitchTimeDuration"].Value);
            if (isQueyMongo != "true")
            {
                if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - QueryConstant.PreviousSwitchTime >=
                switchTimeDurations * 60 * 1000 && QueryConstant.IsSwitch && switchTimeDurations > 0)
                {
                    QueryConstant.IsSwitch = false;
                }
                if (QueryConstant.IsSwitch)
                {
                    var mongoQuery = new MongoDbQuery<T>(_query.Collection, _query.BoolQuery, _query.EsSort, _query.Aggregations, _query.From, _query.Size);
                    var resultMongo = await mongoQuery.GetResult();
                    return resultMongo;
                }
                var elasticsearchQuery = new ElasticsearchQuery<T>(_query.EsClient, _query.BoolQuery, _query.EsSort, _query.Aggregations, _query.From, _query.Size);
                var result = await elasticsearchQuery.GetResult();
                return result;
            }
            else
            {
                var mongoQuery = new MongoDbQuery<T>(_query.Collection, _query.BoolQuery, _query.EsSort, _query.Aggregations, _query.From, _query.Size);
                var result = await mongoQuery.GetResult();
                return result;
            }
        }

        protected override async Task<DatasourceResult<List<T>>> RunFallbackAsync()
        {
            string assemblyPath = new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath;
            Configuration cfg = ConfigurationManager.OpenExeConfiguration(assemblyPath);
            string isQueyMongo = cfg.AppSettings.Settings["AlwaysQueryMongo"].Value;
            if (isQueyMongo != "true")
            {
                QueryConstant.IsSwitch = true;
                QueryConstant.PreviousSwitchTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                var mongoQuery = new MongoDbQuery<T>(_query.Collection, _query.BoolQuery, _query.EsSort, _query.Aggregations, _query.From, _query.Size);
                var result = await mongoQuery.GetResult();
                return result;
            }
            return new DatasourceResult<List<T>>();
        }
    }
}

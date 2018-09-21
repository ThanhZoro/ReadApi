using QueryFailOverEsMongo.Models;
using Steeltoe.CircuitBreaker.Hystrix;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QueryFailOverEsMongo.Query
{
    public class QueryExecute<T> : HystrixCommand<DatasourceResult<List<T>>> where T : class
    {
        private readonly QueryFailOverEsMongo<T> _query;

        public QueryExecute(QueryFailOverEsMongo<T> query) : base(HystrixCommandGroupKeyDefault.AsKey("QueryExecute"), query.QueryTimeOut * 2)
        {
            _query = query;
        }

        protected override async Task<DatasourceResult<List<T>>> RunAsync()
        {
            var selector = new DatabaseSelector<T>(_query);
            var result = await selector.ExecuteAsync();
            return result;
        }

        protected override async Task<DatasourceResult<List<T>>> RunFallbackAsync()
        {
            return new DatasourceResult<List<T>>();
        }
    }
}

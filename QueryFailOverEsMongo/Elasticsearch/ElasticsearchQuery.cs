using Nest;
using QueryFailOverEsMongo.Common;
using QueryFailOverEsMongo.Models;
using QueryFailOverEsMongo.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QueryFailOverEsMongo.Elasticsearch
{
    public class ElasticsearchQuery<T> where T : class
    {
        private ElasticClient _esClient;
        private BoolQuery _boolQuery;
        private List<ISort> _esSort;
        private List<Aggregation> _aggregations;
        private int _from;
        private int _size;

        public ElasticsearchQuery(
            ElasticClient esClient,
            BoolQuery boolQuery,
            List<ISort> esSort,
            List<Aggregation> aggregations,
            int from,
            int size)
        {
            _esClient = esClient;
            _boolQuery = boolQuery;
            _esSort = esSort;
            _aggregations = aggregations;
            _from = from;
            _size = size;
        }

        public async Task<DatasourceResult<List<T>>> GetResult()
        {
            var aggs = new Dictionary<string, AggregationContainer>();
            if (_aggregations.Count > 0)
            {
                foreach (var aggregation in _aggregations)
                {
                    switch (aggregation.AggregationType)
                    {
                        case AggregationType.MAX:
                            {
                                var aggregationContainer = new AggregationContainer();
                                aggregationContainer.Max = new MaxAggregation(aggregation.ColumnName + "_max", aggregation.ColumnName);
                                aggs.Add(aggregation.ColumnName + "_max", aggregationContainer);
                                break;
                            }
                        case AggregationType.MIN:
                            {
                                var aggregationContainer = new AggregationContainer();
                                aggregationContainer.Min = new MinAggregation(aggregation.ColumnName + "_min", aggregation.ColumnName);
                                aggs.Add(aggregation.ColumnName + "_min", aggregationContainer);
                                break;
                            }
                        case AggregationType.AVG:
                            {
                                var aggregationContainer = new AggregationContainer();
                                aggregationContainer.Average = new AverageAggregation(aggregation.ColumnName + "_avg", aggregation.ColumnName);
                                aggs.Add(aggregation.ColumnName + "_avg", aggregationContainer);
                                break;
                            }
                        case AggregationType.SUM:
                            {
                                var aggregationContainer = new AggregationContainer();
                                aggregationContainer.Sum = new SumAggregation(aggregation.ColumnName + "_sum", aggregation.ColumnName);
                                aggs.Add(aggregation.ColumnName + "_sum", aggregationContainer);
                                break;
                            }
                    }
                }
            }

            //sort
            var sortDescriptor = new SortDescriptor<T>();
            foreach (var sort in _esSort)
            {
                sortDescriptor.Field(sort.SortKey, (SortOrder)sort.Order);
            }

            var responseQuery = await _esClient.SearchAsync<T>(s => s
                    .From(_from > 0 ? _from : QueryConstant.DefaultOffset)
                    .Size(_size > QueryConstant.MaxLimit ? QueryConstant.MaxLimit : (_size > 0 ? _size : QueryConstant.MaxLimit))
                    .Query(q => { return _boolQuery; })
                    .Aggregations(aggs)
                    .Sort(so => { return sortDescriptor; })
                );
            if (responseQuery.OriginalException != null || !responseQuery.IsValid)
            {
                throw new Exception((responseQuery.OriginalException != null
                    ? responseQuery.OriginalException.Message
                    : "Query is invalid") + ", switching to MongoDB");
            }
            DatasourceResult<List<T>> result = new DatasourceResult<List<T>>
            {
                From = _from,
                Size = _size
            };
            result.Total = responseQuery.Total;
            result.Data = responseQuery.Documents.ToList();
            foreach(var item in responseQuery.Aggregations)
            {
                if (item.Value is ValueAggregate)
                {
                    var valueAggregate = (ValueAggregate)item.Value;
                    result.AggsResult.Add(item.Key, valueAggregate.Value.Value);
                }
                else
                {
                    result.AggsResult.Add(item.Key, null);
                }
            }

            return result;
        }
    }
}

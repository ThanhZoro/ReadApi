using MongoDB.Bson;
using MongoDB.Driver;
using Nest;
using Newtonsoft.Json.Linq;
using QueryFailOverEsMongo.Common;
using QueryFailOverEsMongo.Extensions;
using QueryFailOverEsMongo.Models;
using QueryFailOverEsMongo.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryFailOverEsMongo.MongoDb
{
    public class MongoDbQueryExecute<T> where T : class
    {
        private readonly IMongoCollection<T> _collection;
        private readonly FilterDefinition<T> _fullQuery;
        private readonly SortDefinition<T> _sort;
        private readonly List<Aggregation> _aggregations;
        private readonly int _from;
        private readonly int _size;

        public MongoDbQueryExecute(
            IMongoCollection<T> collection,
            FilterDefinition<T> fullQuery,
            SortDefinition<T> sort,
            List<Aggregation> aggregations,
            int from,
            int size)
        {
            _collection = collection;
            _fullQuery = fullQuery;
            _sort = sort;
            _aggregations = aggregations;
            _from = from;
            _size = size;
        }

        public Task<DatasourceResult<List<T>>> GetListResult()
        {
            return Task.Run(() =>
            {
                var result = new DatasourceResult<List<T>>
                {
                    From = _from,
                    Size = _size
                };
                var searchResultRaw = _collection.Find<T>(_fullQuery).Sort(_sort);
                result.Total = searchResultRaw.Count();
                result.Data = searchResultRaw.Skip(_from).Limit(_size).ToList();
                return result;
            });
        }

        public Task<Dictionary<string, double?>> GetAggsResult()
        {
            return Task.Run(() =>
            {
                var aggsResult = new Dictionary<string, double?>();
                if (_aggregations.Count != 0)
                {
                    var mongoArgs = _collection.Aggregate().Match(_fullQuery).Group(BuildAggs());
                    var aggsData = mongoArgs.ToCursor();
                    while (aggsData.MoveNext())
                    {
                        var results = aggsData.Current;
                        var aggs = results.FirstOrDefault();
                        if (aggs != null)
                        {
                            var dictionary = aggs.ToDictionary();
                            foreach(var item in dictionary)
                            {
                                if(item.Key != "_id")
                                {
                                    double value;
                                    Double.TryParse(item.Value.ToString(), out value);
                                    aggsResult.Add(item.Key, value);
                                }
                            }
                        }
                    }
                }
                return aggsResult;
            });
        }

        private BsonDocument BuildAggs()
        {
            var aggs = new BsonDocument();
            aggs.Add("_id", new BsonDocument());
            foreach (var aggregation in _aggregations)
            {
                switch (aggregation.AggregationType)
                {
                    case AggregationType.MAX:
                        {
                            aggs.Add(aggregation.ColumnName.UppercaseFirstLetter() + "_max", new BsonDocument { { "$max", "$" + aggregation.ColumnName.UppercaseFirstLetter() } });
                            break;
                        }
                    case AggregationType.MIN:
                        {
                            aggs.Add(aggregation.ColumnName.UppercaseFirstLetter() + "_min", new BsonDocument { { "$min", "$" + aggregation.ColumnName.UppercaseFirstLetter() } });
                            break;
                        }
                    case AggregationType.AVG:
                        {
                            aggs.Add(aggregation.ColumnName.UppercaseFirstLetter() + "_avg", new BsonDocument { { "$avg", "$" + aggregation.ColumnName.UppercaseFirstLetter() } });
                            break;
                        }
                    case AggregationType.SUM:
                        {
                            aggs.Add(aggregation.ColumnName.UppercaseFirstLetter() + "_sum", new BsonDocument { { "$sum", "$" + aggregation.ColumnName.UppercaseFirstLetter() } });
                            break;
                        }
                }
            }
            return aggs;
        }
    }
}

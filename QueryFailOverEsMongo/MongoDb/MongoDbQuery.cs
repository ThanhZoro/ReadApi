using MongoDB.Bson;
using MongoDB.Driver;
using Nest;
using Newtonsoft.Json;
using QueryFailOverEsMongo.Extensions;
using QueryFailOverEsMongo.Models;
using QueryFailOverEsMongo.Query;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QueryFailOverEsMongo.MongoDb
{
    public class MongoDbQuery<T> where T : class
    {
        private IMongoCollection<T> _collection;
        private BoolQuery _boolQuery;
        private List<ISort> _sorts;
        private List<Aggregation> _aggregations;
        private int _from;
        private int _size;

        public MongoDbQuery(
            IMongoCollection<T> collection,
            BoolQuery boolQuery,
            List<ISort> sorts,
            List<Aggregation> aggregations,
            int from,
            int size)
        {
            _collection = collection;
            _boolQuery = boolQuery;
            _sorts = sorts;
            _aggregations = aggregations;
            _from = from;
            _size = size;
        }

        public async Task<DatasourceResult<List<T>>> GetResult()
        {
            var fullQuery = BuildFullQuery();
            var mongoQueryExecute = new MongoDbQueryExecute<T>(_collection, fullQuery, BuildSort(), _aggregations, _from, _size);
            Task<DatasourceResult<List<T>>> listResultTask = mongoQueryExecute.GetListResult();
            var listResultData = listResultTask.Result;
            Task<Dictionary<string, double?>> aggsResult = mongoQueryExecute.GetAggsResult();
            await Task.WhenAll(listResultTask, aggsResult);
            listResultData.AggsResult = aggsResult.Result;
            return listResultData;
        }

        private FilterDefinition<T> BuildFullQuery()
        {
            return Builders<T>.Filter.And(BuildQueryLoop(_boolQuery));
        }

        private FilterDefinition<T> BuildQueryLoop(IBoolQuery boolQuery)
        {
            var filterDefinition = FilterDefinition<T>.Empty;
            var musts = boolQuery.Must;
            var mustNots = boolQuery.MustNot;
            var shoulds = boolQuery.Should;
            if (musts != null)
            {
                foreach (var oneMust in musts)
                {
                    IQueryContainer queryContainer = oneMust;
                    if (queryContainer.Bool != null)
                    {
                        filterDefinition &= BuildQueryLoop(queryContainer.Bool);
                    }
                    filterDefinition &= GenerateQuery(queryContainer);
                }
            }
            if (mustNots != null)
            {
                foreach (var oneMustNot in mustNots)
                {
                    IQueryContainer queryContainer = oneMustNot;
                    if (queryContainer.Bool != null)
                    {
                        filterDefinition = !BuildQueryLoop(queryContainer.Bool) & filterDefinition;
                    }
                    if (filterDefinition == FilterDefinition<T>.Empty)
                    {
                        filterDefinition = !GenerateQuery(queryContainer) & filterDefinition;
                    }
                    else
                    {
                        filterDefinition &= !GenerateQuery(queryContainer);
                    }
                }
            }
            if (shoulds != null)
            {
                foreach (var oneShould in shoulds)
                {
                    IQueryContainer queryContainer = oneShould;
                    if (queryContainer.Bool != null)
                    {
                        filterDefinition |= BuildQueryLoop(queryContainer.Bool);
                    }
                    if (filterDefinition == FilterDefinition<T>.Empty)
                    {
                        filterDefinition &= GenerateQuery(queryContainer);
                    }
                    else
                    {
                        filterDefinition |= GenerateQuery(queryContainer);
                    }
                }
            }
            return filterDefinition;
        }

        private FilterDefinition<T> GenerateQuery(IQueryContainer queryContainer)
        {
            if (queryContainer.Term != null)
            {
                return Builders<T>.Filter.Eq(queryContainer.Term.Field.Name.UppercaseFirstLetter(), queryContainer.Term.Value);
            }
            if (queryContainer.Terms != null)
            {
                return Builders<T>.Filter.In(queryContainer.Terms.Field.Name.UppercaseFirstLetter(), queryContainer.Terms.Terms);
            }
            if (queryContainer.Range != null)
            {
                var range = (NumericRangeQuery)queryContainer.Range;
                var rangeQuery = Builders<T>.Filter.Empty;
                if (range.GreaterThan != null)
                {
                    rangeQuery &= Builders<T>.Filter.Gt(range.Field.Name.UppercaseFirstLetter(), range.GreaterThan);
                }
                if (range.GreaterThanOrEqualTo != null)
                {
                    rangeQuery &= Builders<T>.Filter.Gte(range.Field.Name.UppercaseFirstLetter(), range.GreaterThanOrEqualTo);
                }
                if (range.LessThan != null)
                {
                    rangeQuery &= Builders<T>.Filter.Lt(range.Field.Name.UppercaseFirstLetter(), range.LessThan);
                }
                if (range.LessThanOrEqualTo != null)
                {
                    rangeQuery &= Builders<T>.Filter.Lte(range.Field.Name.UppercaseFirstLetter(), range.LessThanOrEqualTo);
                }
                return rangeQuery;
            }
            if (queryContainer.Match != null)
            {
                var stringValue = queryContainer.Match.Query;
                var stringArr = stringValue.Split(' ');
                var value = new BsonRegularExpression(string.Format("^(.*?(\\b{0}\\b)[^$]*)$", stringArr[0]), "img");
                var filter = Builders<T>.Filter.Regex(queryContainer.Match.Field.Name.UppercaseFirstLetter(), value);
                for (int i = 1; i < stringArr.Length; i++)
                {
                    value = new BsonRegularExpression("^(.*?(\\b" + stringArr[i] + "\\b)[^$]*)$", "img");
                    filter |= Builders<T>.Filter.Regex(queryContainer.Match.Field.Name.UppercaseFirstLetter(), value);
                }
                return filter;
            }
            if (queryContainer.MatchPhrase != null)
            {
                var stringValue = queryContainer.MatchPhrase.Query;
                var stringArr = stringValue.Split(' ');
                var value = new BsonRegularExpression(string.Format("^(.*?(\\b{0}\\b)[^$]*)$", stringArr[0]), "img");
                var filter = Builders<T>.Filter.Regex(queryContainer.MatchPhrase.Field.Name.UppercaseFirstLetter(), value);
                for (int i = 1; i < stringArr.Length; i++)
                {
                    value = new BsonRegularExpression("^(.*?(\\b" + stringArr[i] + "\\b)[^$]*)$", "img");
                    filter &= Builders<T>.Filter.Regex(queryContainer.MatchPhrase.Field.Name.UppercaseFirstLetter(), value);
                }
                return filter;
            }
            if (queryContainer.Wildcard != null)
            {
                var stringValue = queryContainer.Wildcard.Value.ToString();
                var value = new BsonRegularExpression(stringValue.Replace("*", ""), "img");
                return Builders<T>.Filter.Regex(queryContainer.Wildcard.Field.Name.UppercaseFirstLetter(), value);
            }
            if(queryContainer.RawQuery != null)
            {
                var rawString = queryContainer.RawQuery.Raw;
                var rawQuery = RawQueryConverter.ConvertEsToMongoQuery(rawString);
                var querySearch = BsonDocument.Parse(JsonConvert.SerializeObject(rawQuery));
                return querySearch;
            }
            return Builders<T>.Filter.Empty;
        }

        private SortDefinition<T> BuildSort()
        {
            SortDefinition<T> sort = null;
            if (_sorts != null)
            {
                foreach (var oneSort in _sorts)
                {
                    if (oneSort.Order == SortOrder.Ascending)
                        sort.Ascending(oneSort.SortKey.Name.UppercaseFirstLetter());
                    else
                        sort.Descending(oneSort.SortKey.Name.UppercaseFirstLetter());
                }
            }
            return sort;
        }
    }
}

using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using QueryFailOverEsMongo.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QueryFailOverEsMongo.MongoDb
{
    public class RawQueryConverter
    {
        public static JObject ConvertEsToMongoQuery(string esString)
        {
            JObject queryObject = JObject.Parse(esString);
            return Convert(queryObject);
        }
        private static JObject Convert(JObject jObject)
        {
            var boolQuery = jObject["bool"];
            if (boolQuery != null)
            {
                var fullQuery = new JObject();
                JArray and = new JArray();
                JArray or = new JArray();
                JArray nand = new JArray();
                var query = BuildQueryLoop(boolQuery);
                foreach (JProperty item in query.Children())
                {
                    switch (item.Name)
                    {
                        case "$and":
                            and = item.Value as JArray;
                            break;
                        case "$or":
                            or = item.Value as JArray;
                            break;
                        case "$nand":
                            nand = item.Value as JArray;
                            break;
                    }
                }
                if (nand.Count > 0)
                {
                    and.Merge(nand);
                }
                if (and != null)
                    fullQuery.Add(new JProperty("$and", and));
                if (or.Count > 0)
                    fullQuery.Add(new JProperty("$or", or));
                return fullQuery;
            }
            return new JObject();
        }

        private static JObject BuildQueryLoop(JToken jToken)
        {
            var fullQuery = new JObject();
            foreach (JProperty item in jToken.Children())
            {
                if (item.Name == "must")
                {
                    var arrayMust = new JArray();
                    foreach (var obj in item.Value.Children())
                    {
                        var c = obj.First as JProperty;
                        if (c.Name != "bool")
                        {
                            arrayMust.Add(GenerateQuery(c));
                        }
                        else
                        {
                            var x = BuildQueryLoop(c.Value);
                            arrayMust.Add(x);
                        }
                    }
                    var propertyMust = new JProperty("$and", arrayMust);
                    fullQuery.Add(propertyMust);
                }
                if (item.Name == "should")
                {
                    var arrayShould = new JArray();
                    foreach (var obj in item.Value.Children())
                    {
                        var c = obj.First as JProperty;
                        if (c.Name != "bool")
                        {
                            arrayShould.Add(GenerateQuery(c));
                        }
                        else
                        {
                            var x = BuildQueryLoop(c.Value);
                            arrayShould.Add(x);
                        }
                    }
                    var propertShould = new JProperty("$or", arrayShould);
                    fullQuery.Add(propertShould);
                }

                if (item.Name == "must_not")
                {
                    var arrayMustNot = new JArray();
                    foreach (var obj in item.Value.Children())
                    {
                        var c = obj.First as JProperty;
                        if (c.Name != "bool")
                        {
                            arrayMustNot.Add(GenerateQuery(c, true));
                        }
                        else
                        {
                            var x = BuildQueryLoop(c.Value);
                            arrayMustNot.Add(x);
                        }
                    }
                    var propertyMust = new JProperty("$nand", arrayMustNot);
                    fullQuery.Add(propertyMust);
                }
            }
            return fullQuery;
        }

        private static JObject GenerateQuery(JProperty jProperty, bool isNot = false)
        {
            var item = jProperty.Value.Children().FirstOrDefault() as JProperty;
            switch (jProperty.Name)
            {
                case "term":
                    if (!isNot)
                    {
                        return new JObject
                        {
                            { item.Name.UppercaseFirstLetter(), item.Value}
                        };
                    }
                    else
                    {
                        return new JObject
                        {
                            { item.Name.UppercaseFirstLetter(),
                                new JObject()
                                {
                                    { "$ne", item.Value }
                                }
                            }
                        };
                    }
                case "terms":
                    if (!isNot)
                    {
                        return new JObject
                        {
                            {
                                item.Name.UppercaseFirstLetter(), new JObject
                                {
                                    { "$in", item.Value}
                                }
                            }
                        };
                    }
                    else
                    {
                        return new JObject
                        {
                            {
                                item.Name.UppercaseFirstLetter(), new JObject
                                {
                                    { "$nin", item.Value}
                                }
                            }
                        };
                    }
                case "range":
                    var ranges = item.Value;
                    var from = string.Empty;
                    var to = string.Empty;
                    var objRange = new JObject();
                    foreach (JProperty r in ranges)
                    {
                        DateTime dateTime;
                        bool isDateTime = DateTime.TryParse(r.Value.ToString(), out dateTime);
                        if (isDateTime)
                        {
                            var queryJson = new JObject()
                            {
                                {"$date", r.Value }
                            };
                            switch (r.Name)
                            {
                                case "lt":
                                    objRange.Add(new JProperty("$lt", queryJson));
                                    break;
                                case "lte":
                                    objRange.Add(new JProperty("$lte", queryJson));
                                    break;
                                case "gt":
                                    objRange.Add(new JProperty("$gt", queryJson));
                                    break;
                                case "gte":
                                    objRange.Add(new JProperty("$gte", queryJson));
                                    break;
                            }
                        }
                        else
                        {
                            switch (r.Name)
                            {
                                case "lt":
                                    objRange.Add(new JProperty("$lt", r.Value));
                                    break;
                                case "lte":
                                    objRange.Add(new JProperty("$lte", r.Value));
                                    break;
                                case "gt":
                                    objRange.Add(new JProperty("$gt", r.Value));
                                    break;
                                case "gte":
                                    objRange.Add(new JProperty("$gte", r.Value));
                                    break;
                            }
                        }
                    }
                    if (!isNot)
                    {
                        return new JObject
                        {
                            {
                                item.Name.UppercaseFirstLetter(), objRange
                            }
                        };
                    }
                    else
                    {
                        return new JObject
                        {
                            {
                                item.Name.UppercaseFirstLetter(),
                                new JObject()
                                {
                                    { "$not", objRange }
                                }
                            }
                        };
                    }
                case "query_string":
                    var queryString = jProperty.Value.Children();
                    var defaultField = "";
                    var query = "";
                    foreach (JProperty property in queryString)
                    {
                        if (property.Name == "default_field")
                        {
                            defaultField = property.Value.ToString().UppercaseFirstLetter();
                        }
                        if (property.Name == "query")
                        {
                            query = property.Value.ToString();
                            if (query[0] == '*')
                                query = query.Remove(0, 1);
                            if (query[query.Length - 1] == '*')
                                query = query.Remove(query.Length - 1);
                        }
                    }
                    return new JObject
                    {
                        {
                            defaultField, new JObject
                            {
                                { "$regex", query}
                            }
                        }
                    };
                case "ids":
                    var objectIds = new JArray();
                    foreach(var id in item.Value.Children())
                    {
                        var obj = new JObject()
                        {
                            { "$oid",  id.ToString()}
                        };
                        objectIds.Add(obj);
                    }
                    if (!isNot)
                    {
                        return new JObject
                        {
                            {
                                "_id", new JObject
                                {
                                    { "$in", objectIds}
                                }
                            }
                        };
                    }
                    else
                    {
                        return new JObject
                        {
                            {
                                "_id", new JObject
                                {
                                    { "$nin", objectIds}
                                }
                            }
                        };
                    }
            }
            return new JObject();
        }
    }
}
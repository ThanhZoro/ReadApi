namespace QueryFailOverEsMongo.Query
{
    public class Aggregation
    {
        public AggregationType AggregationType { get; set; }
        public string ColumnName { get; set; }
    }

    public enum AggregationType
    {
        MIN,
        MAX,
        AVG,
        SUM
    }
}
using System.Collections.Generic;

namespace QueryFailOverEsMongo.Models
{
    public class DatasourceResult<T>
    {
        public int From { get; set; }
        public int Size { get; set; }
        public long Total { get; set; }
        public T Data { get; set; }
        public Dictionary<string, double?> AggsResult { get; set; } = new Dictionary<string, double?>();
    }
}

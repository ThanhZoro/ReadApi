using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ReadApi.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class ElasticSearchQuery
    {
        /// <summary>
        /// 
        /// </summary>
        public int From { get; set; } = 0;
        /// <summary>
        /// 
        /// </summary>
        [Range(1, 100, ErrorMessage = "sizeLengthInvalid")]
        public int Size { get; set; } = 10;
        /// <summary>
        /// 
        /// </summary>
        public object Query { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ElasticSearchSort Sort { get; set; } = new ElasticSearchSort();
        /// <summary>
        /// 
        /// </summary>
        public Source Source { get; set; } = new Source();
    }

    /// <summary>
    /// 
    /// </summary>
    public class Source
    {
        /// <summary>
        /// 
        /// </summary>
        public List<string> Includes { get; set; } = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        public List<string> Excludes { get; set; } = new List<string>();
    }


    /// <summary>
    /// 
    /// </summary>
    public class ElasticSearchSort
    {
        /// <summary>
        /// 
        /// </summary>
        public string Field { get; set; } = "createdAt";
        /// <summary>
        /// 
        /// </summary>
        public int SortOrder { get; set; } = 1;
    }
}
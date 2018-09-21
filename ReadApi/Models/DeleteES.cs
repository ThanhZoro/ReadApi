using System.Collections.Generic;

namespace ReadApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class DeleteES
    {
        /// <summary>
        /// 
        /// </summary>
        public string Index { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> Ids { get; set; } = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        public string Type { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Item
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }
    }
}

using System.Collections.Generic;

namespace ReadApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class GetActivityHistoryLead
    {
        /// <summary>
        /// 
        /// </summary>
        public string LeadId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> Type { get; set; } = new List<string>();
    }
}

using Contracts.Models;

namespace ReadApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class LeadResponse : Lead
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsSurveyed { get; set; }
    }
}

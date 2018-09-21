using Nest;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReadApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public interface IReportRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<Dictionary<string, Dictionary<string, long?>>> LeadReport();
    }
}

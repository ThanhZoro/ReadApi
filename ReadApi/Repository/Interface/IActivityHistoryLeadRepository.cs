using Contracts.Models;
using ReadApi.Data;
using ReadApi.Models;
using QueryFailOverEsMongo.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReadApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public interface IActivityHistoryLeadRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<List<ActivityHistoryLead>> Get(GetActivityHistoryLead data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<DatasourceResult<List<ActivityHistoryLead>>> GetByQuery(ElasticSearchQuery query);
    }
}

using Contracts.Models;
using ReadApi.Data;
using QueryFailOverEsMongo.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReadApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public interface IChatLeadRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="leadId"></param>
        /// <returns></returns>
        Task<List<ChatLead>> Get(string leadId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<DatasourceResult<List<ChatLead>>> GetByQuery(ElasticSearchQuery query);
    }
}

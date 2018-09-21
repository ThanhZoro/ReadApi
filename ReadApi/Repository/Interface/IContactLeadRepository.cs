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
    public interface IContactLeadRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="leadId"></param>
        /// <returns></returns>
        Task<List<ContactLead>> Get(string leadId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ContactLead> GetById(string id);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<DatasourceResult<List<ContactLead>>> GetByQuery(ElasticSearchQuery query);
    }
}

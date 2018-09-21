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
    public interface ILeadRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<DatasourceResult<List<Lead>>> GetByQuery(ElasticSearchQuery query);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Lead> GetById(string id);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="language"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<string> Export(ElasticSearchQuery query, string language, string userId);
    }
}

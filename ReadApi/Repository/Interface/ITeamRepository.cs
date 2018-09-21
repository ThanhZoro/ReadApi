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
    public interface ITeamRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Team> GetById(string id);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<DatasourceResult<List<Team>>> GetByQuery(ElasticSearchQuery query);
    }
}

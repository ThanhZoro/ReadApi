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
    public interface ITeamUsersRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TeamUsers> GetById(string id);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<DatasourceResult<List<TeamUsers>>> GetByQuery(ElasticSearchQuery query);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<string>> GetTeamIds(string companyId, string userId);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<List<TeamUsers>> GetAll();
    }
}

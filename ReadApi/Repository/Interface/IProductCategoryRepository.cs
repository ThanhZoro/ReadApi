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
    public interface IProductCategoryRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ProductCategory> GetById(string id);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<DatasourceResult<List<ProductCategory>>> GetByQuery(ElasticSearchQuery query);
    }
}

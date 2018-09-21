using Contracts.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReadApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICommonDataRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<List<CommonData>> GetAll();
    }
}

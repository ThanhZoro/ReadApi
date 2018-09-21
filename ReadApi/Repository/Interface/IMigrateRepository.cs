using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReadApi.Models;

namespace ReadApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMigrateRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        Task<long> Migrate(string index, string companyId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<List<string>> DeleteES(DeleteES data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        Task<string> DeleteIndexES(string index);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task Reindex(bool refreshAllIndex, string refreshIndex, string companyId);
    }
}

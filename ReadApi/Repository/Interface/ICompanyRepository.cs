using Contracts.Models;
using System.Threading.Tasks;

namespace ReadApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICompanyRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<Company> GetById();
        Task<Company> GetByExistId(string id);
        Task<string> GetIdByCode(string code);
    }
}

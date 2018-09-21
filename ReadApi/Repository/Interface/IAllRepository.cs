using System;
using System.Threading.Tasks;

namespace ReadApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAllRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<object> GetAll();
    }
}


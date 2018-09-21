using Contracts.Models;
using System.Threading.Tasks;

namespace ReadApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class AllRepository : IAllRepository
    {
        private readonly ICommonDataRepository _commonDataRepository;
        private readonly IAccountRepository _accountRepository;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commonDataRepository"></param>
        /// <param name="accountRepository"></param>
        public AllRepository(
            ICommonDataRepository commonDataRepository,
            IAccountRepository accountRepository)
        {
            _commonDataRepository = commonDataRepository;
            _accountRepository = accountRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<object> GetAll()
        {
            var commonData = await _commonDataRepository.GetAll();
            //users
            var listUsers = await _accountRepository.GetAll();
            return new
            {
                CommonData = commonData,
                Users = listUsers,
                Roles = new RolesList().GetListSort()
            };
        }
    }
}


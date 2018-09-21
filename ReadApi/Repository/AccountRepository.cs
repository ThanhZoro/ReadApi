using Contracts.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReadApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class AccountRepository : IAccountRepository
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAccessRightRepository _accessRightRepository;
        private readonly ITeamUsersRepository _teamUsersRepository;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="accessRightRepository"></param>
        /// <param name="teamUsersRepository"></param>
        public AccountRepository(
            UserManager<ApplicationUser> userManager, 
            IHttpContextAccessor httpContextAccessor,
            IAccessRightRepository accessRightRepository,
            ITeamUsersRepository teamUsersRepository)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _accessRightRepository = accessRightRepository;
            _teamUsersRepository = teamUsersRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<UserViewModel>> GetAll()
        {
            var companyId = _httpContextAccessor.HttpContext.Request?.Headers["CompanyId"].FirstOrDefault();
            var listRoles = _accessRightRepository.AllRoles(companyId).Result ?? new List<UserRoles>();
            var teamUsers = await _teamUsersRepository.GetAll();
            var listUsers = _userManager.Users.Where(s => s.Companies != null && s.Companies.Contains(companyId)).Select(user => new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Position = user.Position,
                Gender = user.Gender,
                Birthday = user.Birthday,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                Code = user.Code,
                Department = user.Department,
                Address = user.Address,
                CreatedAt = user.CreatedAt,
                CreatedBy = user.CreatedBy,
                UpdatedAt = user.UpdatedAt,
                UpdatedBy = user.UpdatedBy,
                RequiredChangePassword = user.RequiredChangePassword
            }).ToList();
            foreach (var item in listUsers.ToList())
            {
                item.Roles = listRoles.FirstOrDefault(s => s.UserId == item.Id) != null ? listRoles.FirstOrDefault(s => s.UserId == item.Id).Roles : new List<string>();
                item.TeamIds = teamUsers.FirstOrDefault(f => f.UserId == item.Id) != null ? teamUsers.FirstOrDefault(f => f.UserId == item.Id).TeamIds : new List<string>();
            };
            return listUsers;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<ApplicationUser> GetUsers(List<string> ids)
        {
            var result = _userManager.Users.Where(s => ids.Contains(s.Id)).ToList();
            return result;
        }
    }
}

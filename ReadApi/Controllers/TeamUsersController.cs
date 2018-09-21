using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TNTMgmt.Authorization;
using ReadApi.Data;
using ReadApi.Repository;
using QueryFailOverEsMongo.Models;

namespace ReadApi.Controllers
{
    /// <summary>
    /// summary for TeamController
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/read/[controller]/[action]")]
    public class TeamUsersController : Controller
    {
        private readonly ITeamUsersRepository _teamUsersRepository;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamUsersRepository"></param>
        public TeamUsersController(ITeamUsersRepository teamUsersRepository)
        {
            _teamUsersRepository = teamUsersRepository;
        }

        /// <summary>
        /// get team users by query
        /// </summary>
        /// <param name="query"></param>
        /// <returns>list team users</returns>
        /// <returns>return list team users</returns>
        [HttpPost]
        [AccessRight("SETTINGS_VIEW")]
        [ProducesResponseType(typeof(DatasourceResult<List<TeamUsers>>), 200)]
        public async Task<IActionResult> GetByQuery([FromBody]ElasticSearchQuery query)
        {
            var result = await _teamUsersRepository.GetByQuery(query);
            return Ok(result);
        }

        /// <summary>
        /// get team users by id
        /// </summary>
        /// <param name="id">id of team users</param>
        /// <returns>a team users</returns>
        /// <returns>return a team users</returns>
        [HttpGet]
        [AccessRight("SETTINGS_VIEW")]
        [ProducesResponseType(typeof(TeamUsers), 200)]
        public async Task<IActionResult> GetById([FromQuery]string id)
        {
            var result = await _teamUsersRepository.GetById(id);
            return Ok(result);
        }
    }
}
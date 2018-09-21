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
    public class TeamController : Controller
    {
        private readonly ITeamRepository _teamRepository;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamRepository"></param>
        public TeamController(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        /// <summary>
        /// get teams by query
        /// </summary>
        /// <param name="query"></param>
        /// <returns>list teams</returns>
        /// <returns>return list teams</returns>
        [HttpPost]
        [AccessRight("SETTINGS_VIEW")]
        [ProducesResponseType(typeof(DatasourceResult<List<Team>>), 200)]
        public async Task<IActionResult> GetByQuery([FromBody]ElasticSearchQuery query)
        {
            var result = await _teamRepository.GetByQuery(query);
            return Ok(result);
        }

        /// <summary>
        /// get team by id
        /// </summary>
        /// <param name="id">id of team</param>
        /// <returns>a team</returns>
        /// <returns>return a team</returns>
        [HttpGet]
        [AccessRight("SETTINGS_VIEW")]
        [ProducesResponseType(typeof(Team), 200)]
        public async Task<IActionResult> GetById([FromQuery]string id)
        {
            var result = await _teamRepository.GetById(id);
            return Ok(result);
        }
    }
}
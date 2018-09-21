using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TNTMgmt.Authorization;
using ReadApi.Data;
using ReadApi.Models;
using ReadApi.Repository;

namespace ReadApi.Controllers
{
    /// <summary>
    /// summary for ActivityHistoryLeadController
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/read/[controller]/[action]")]
    public class ActivityHistoryLeadController : Controller
    {
        private readonly IActivityHistoryLeadRepository _activityHistoryLeadRepository;

        /// <summary>
        /// contructor ActivityHistoryLeadController
        /// </summary>
        /// <param name="activityHistoryLeadRepository"></param>
        public ActivityHistoryLeadController(IActivityHistoryLeadRepository activityHistoryLeadRepository)
        {
            _activityHistoryLeadRepository = activityHistoryLeadRepository;
        }

        /// <summary>
        /// get activity hostory lead
        /// </summary>
        /// <param name="data">data filter info</param>
        /// <returns>list activity history leads</returns>
        /// <response code="200">returns list activity history leads</response>
        [HttpPost]
        [AccessRight("LEAD_VIEW")]
        [ProducesResponseType(typeof(List<ActivityHistoryLead>), 200)]
        public async Task<IActionResult> Get([FromBody]GetActivityHistoryLead data)
        {
            var result = await _activityHistoryLeadRepository.Get(data);
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [AccessRight("LEAD_VIEW")]
        public async Task<IActionResult> GetByQuery([FromBody]ElasticSearchQuery data)
        {
            var result = await _activityHistoryLeadRepository.GetByQuery(data);
            return Ok(result);
        }
    }
}
using Contracts.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TNTMgmt.Authorization;
using ReadApi.Data;
using ReadApi.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReadApi.Controllers
{
    /// <summary>
    /// summary for ChatLeadController
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/read/[controller]/[action]")]
    public class ChatLeadController : Controller
    {
        private readonly IChatLeadRepository _chatLeadRepository;

        /// <summary>
        /// contructor ChatLeadController
        /// </summary>
        /// <param name="chatLeadRepository"></param>
        public ChatLeadController(IChatLeadRepository chatLeadRepository)
        {
            _chatLeadRepository = chatLeadRepository;
        }

        /// <summary>
        /// get list chat leads by lead id
        /// </summary>
        /// <param name="leadId">id of lead</param>
        /// <returns>list chat leads</returns>
        /// <response code="200">returns list chat leads</response>
        [HttpGet]
        [AccessRight("LEAD_VIEW")]
        [ProducesResponseType(typeof(List<ChatLead>), 200)]
        public async Task<IActionResult> Get([FromQuery]string leadId)
        {
            var result = await _chatLeadRepository.Get(leadId);
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        [AccessRight("LEAD_VIEW")]
        public async Task<IActionResult> GetByQuery([FromBody]ElasticSearchQuery query)
        {
            var data = await _chatLeadRepository.GetByQuery(query);
            return Ok(data);
        }
    }
}

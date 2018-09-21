using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TNTMgmt.Authorization;
using ReadApi.Data;
using ReadApi.Repository;

namespace ReadApi.Controllers
{
    /// <summary>
    /// summary for ContactLeadController
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/read/[controller]/[action]")]
    public class ContactLeadController : Controller
    {
        private readonly IContactLeadRepository _contactLeadRepository;

        /// <summary>
        /// contructor ContactLeadController
        /// </summary>
        /// <param name="contactLeadRepository"></param>
        public ContactLeadController(IContactLeadRepository contactLeadRepository)
        {
            _contactLeadRepository = contactLeadRepository;
        }

        /// <summary>
        /// get contact lead by lead id
        /// </summary>
        /// <param name="leadId">id of lead</param>
        /// <returns>list contact leads</returns>
        /// <response code="200">returns list contact leads</response>
        [HttpGet]
        [AccessRight("LEAD_VIEW")]
        [ProducesResponseType(typeof(List<ContactLead>), 200)]
        public async Task<IActionResult> Get([FromQuery]string leadId)
        {
            var result = await _contactLeadRepository.Get(leadId);
            return Ok(result);
        }

        /// <summary>
        /// get contact lead by id
        /// </summary>
        /// <param name="id">if of contact lead</param>
        /// <returns>the contact lead</returns>
        /// <response code="200">returns the contact lead</response>
        [HttpGet]
        [AccessRight("LEAD_VIEW")]
        [ProducesResponseType(typeof(ContactLead), 200)]
        public async Task<IActionResult> GetById([FromQuery]string id)
        {
            var result = await _contactLeadRepository.GetById(id);
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
            var data = await _contactLeadRepository.GetByQuery(query);
            return Ok(data);
        }
    }
}
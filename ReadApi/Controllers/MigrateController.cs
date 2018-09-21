using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadApi.Models;
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
    public class MigrateController : Controller
    {
        private readonly IMigrateRepository _migrateRepository;

        /// <summary>
        /// contructor MigrateController
        /// </summary>
        /// <param name="migrateRepository"></param>
        public MigrateController(IMigrateRepository migrateRepository)
        {
            _migrateRepository = migrateRepository;
        }

        /// <summary>
        /// migrate data from mongo to es
        /// </summary>
        /// <param name="companyId">company id from header</param>
        /// <param name="index">index</param>
        /// <returns>total documents indexed</returns>
        /// <response code="200">total documents indexed</response>
        [HttpGet]
        [ProducesResponseType(typeof(long), 200)]
        public async Task<IActionResult> Migrate([FromQuery]string companyId, [FromQuery]string index)
        {
            var result = await _migrateRepository.Migrate(index, companyId);
            return Ok(result);
        }

        /// <summary>
        /// delete documents in es by id
        /// </summary>
        /// <param name="data">delete info</param>
        /// <returns>list ids deleted</returns>
        /// <response code="200">returns list ids deleted</response>
        [HttpDelete]
        [ProducesResponseType(typeof(List<string>), 200)]
        public async Task<IActionResult> DeleteES([FromBody]DeleteES data)
        {
            if (ModelState.IsValid)
            {
                var result = await _migrateRepository.DeleteES(data);
                return Ok(result);
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// delete index in es
        /// </summary>
        /// <param name="index">index</param>
        /// <returns>index deleted</returns>
        /// <response code="200">returns index deleted</response>
        [HttpDelete]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> DeleteIndexES([FromQuery]string index)
        {
            var result = await _migrateRepository.DeleteIndexES(index);
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="refreshAllIndex"></param>
        /// <param name="refreshIndex"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Reindex([FromQuery]bool refreshAllIndex, [FromQuery]string refreshIndex, [FromQuery]string companyId)
        {
            await _migrateRepository.Reindex(refreshAllIndex, refreshIndex, companyId);
            return Ok();
        }
    }
}

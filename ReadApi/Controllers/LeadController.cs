using Contracts.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TNTMgmt.Authorization;
using ReadApi.Data;
using ReadApi.Repository;
using QueryFailOverEsMongo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ReadApi.Controllers
{
    /// <summary>
    /// summary for LeadController
    /// </summary>
    [Authorize]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/read/[controller]/[action]")]
    public class LeadController : Controller
    {
        private ILeadRepository _leadRepository;

        /// <summary>
        /// contructor LeadController
        /// </summary>
        /// <param name="leadRepository"></param>
        public LeadController(ILeadRepository leadRepository)
        {
            _leadRepository = leadRepository;
        }

        /// <summary>
        /// get lead by query
        /// </summary>
        /// <param name="query">query info</param>
        /// <returns>list leads</returns>
        /// <response code="200">returns list leads</response>
        [HttpPost]
        [AccessRight("LEAD_VIEW")]
        [ProducesResponseType(typeof(DatasourceResult<List<Lead>>), 200)]
        public async Task<IActionResult> GetByQuery([FromBody]ElasticSearchQuery query)
        {
            var data = await _leadRepository.GetByQuery(query);
            return Ok(data);
        }

        /// <summary>
        /// get lead by id
        /// </summary>
        /// <param name="id">id of lead</param>
        /// <returns>the lead</returns>
        /// <response code="200">returns the lead</response>
        [HttpGet]
        [AccessRight("LEAD_VIEW")]
        [ProducesResponseType(typeof(Lead), 200)]
        public async Task<IActionResult> GetById([FromQuery]string id)
        {
            var data = await _leadRepository.GetById(id);
            return Ok(data);
        }

        /// <summary>
        /// export excel leads
        /// </summary>
        /// <param name="query">query info</param>
        /// <param name="language">language need export</param>
        /// <returns>file excel</returns>
        [HttpPost]
        [AccessRight("LEAD_EXPORT")]
        public async Task<IActionResult> Export([FromBody]ElasticSearchQuery query, [FromQuery]string language)
        {
            var userId = User.Claims.FirstOrDefault(s => s.Type == "sub").Value;
            var result = await _leadRepository.Export(query, language, userId);

            var path = Path.Combine(
                           Directory.GetCurrentDirectory(),
                           "wwwroot/Export", result);

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, GetContentType(path), $"{Path.GetFileName(path)}_{DateTime.Now.ToString("yyyyMMddHHmmss")}");
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformatsofficedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"}
            };
        }
    }
}

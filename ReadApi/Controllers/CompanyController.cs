using Contracts.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadApi.Repository;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ReadApi.Controllers
{
    /// <summary>
    /// summary for CompanyController
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/read/[controller]/[action]")]
    public class CompanyController : Controller
    {
        private ICompanyRepository _companyRepository;

        /// <summary>
        /// contructor CompanyController
        /// </summary>
        /// <param name="companyRepository"></param>
        public CompanyController(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetTimeZone()
        {
            var listTimeZone = TimeZoneInfo.GetSystemTimeZones().Select(s => new { id = s.Id, name = s.DisplayName });
            return Ok(listTimeZone);
        }

        /// <summary>
        /// get company by id
        /// </summary>
        /// <returns>the company info</returns>
        /// <response code="200">returns the company info</response>
        [HttpGet]
        [ProducesResponseType(typeof(Company), 200)]
        public async Task<IActionResult> GetById()
        {
            var data = await _companyRepository.GetById();
            return Ok(data);
        }

        /// <summary>
        /// get id of company by code
        /// </summary>
        /// <param name="code">code of company</param>
        /// <returns>id of company</returns>
        /// <response code="200">returns id of company</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> GetIdByCode([FromQuery]string code)
        {
            var result = await _companyRepository.GetIdByCode(code);
            if (!string.IsNullOrEmpty(result))
                return Ok(result);
            else
                return BadRequest();
        }
    }
}

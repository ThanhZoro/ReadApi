using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TNTMgmt.Authorization;
using ReadApi.Repository;

namespace ReadApi.Controllers
{
    /// <summary>
    /// summary for LeadController
    /// </summary>
    [Authorize]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/read/[controller]")]
    public class ReportController : Controller
    {
        private readonly IReportRepository _reportRepository;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reportRepository"></param>
        public ReportController(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        ///
        [HttpGet("lead")]
        [AccessRight("LEAD_VIEW")]
        public async Task<IActionResult> LeadReport()
        {
            var data = await _reportRepository.LeadReport();
            return Ok(data);
        }
    }
}
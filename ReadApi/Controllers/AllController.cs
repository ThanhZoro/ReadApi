using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadApi.Repository;
using System.Threading.Tasks;

namespace ReadApi.Controllers
{
    /// <summary>
    /// summary for AllController
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/read/[controller]/[action]")]
    public class AllController : Controller
    {
        private readonly IAllRepository _allRepository;

        /// <summary>
        /// contructor AllController
        /// </summary>
        /// <param name="allRepository"></param>
        public AllController(IAllRepository allRepository)
        {
            _allRepository = allRepository;
        }

        /// <summary>
        /// get all users + common datas by company id
        /// </summary>
        /// <returns>a object contains list users and common datas</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _allRepository.GetAll();
            return Ok(result);
        }
    }
}

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
    /// summary for ProductController
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/read/[controller]/[action]")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productRepository"></param>
        public ProductController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        /// <summary>
        /// get products by query
        /// </summary>
        /// <param name="query"></param>
        /// <returns>list products</returns>
        /// <returns>return list products</returns>
        [HttpPost]
        [AccessRight("PRODUCT_VIEW")]
        [ProducesResponseType(typeof(DatasourceResult<List<Product>>), 200)]
        public async Task<IActionResult> GetByQuery([FromBody]ElasticSearchQuery query)
        {
            var result = await _productRepository.GetByQuery(query);
            return Ok(result);
        }

        /// <summary>
        /// get product by id
        /// </summary>
        /// <param name="id">id of product</param>
        /// <returns>a product</returns>
        /// <returns>return a product</returns>
        [HttpGet]
        [AccessRight("PRODUCT_VIEW")]
        [ProducesResponseType(typeof(Product), 200)]
        public async Task<IActionResult> GetById([FromQuery]string id)
        {
            var result = await _productRepository.GetById(id);
            return Ok(result);
        }
    }
}

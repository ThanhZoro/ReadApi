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
    /// summary for ProductCategoryController
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/read/[controller]/[action]")]
    public class ProductCategoryController : Controller
    {
        private readonly IProductCategoryRepository _productCategoryRepository;

        /// <summary>
        /// contructor ProductCategoryController
        /// </summary>
        /// <param name="productCategoryRepository"></param>
        public ProductCategoryController(IProductCategoryRepository productCategoryRepository)
        {
            _productCategoryRepository = productCategoryRepository;
        }

        /// <summary>
        /// get product categories by query
        /// </summary>
        /// <param name="query"></param>
        /// <returns>list product categories</returns>
        /// <returns>return list product categories</returns>
        [HttpPost]
        [AccessRight("PRODUCT_VIEW")]
        [ProducesResponseType(typeof(DatasourceResult<List<ProductCategory>>), 200)]
        public async Task<IActionResult> GetByQuery([FromBody]ElasticSearchQuery query)
        {
            var result = await _productCategoryRepository.GetByQuery(query);
            return Ok(result);
        }

        /// <summary>
        /// get product category by id
        /// </summary>
        /// <param name="id">id of product category</param>
        /// <returns>a product category</returns>
        /// <returns>return a product category</returns>
        [HttpGet]
        [AccessRight("PRODUCT_VIEW")]
        [ProducesResponseType(typeof(ProductCategory), 200)]
        public async Task<IActionResult> GetById([FromQuery]string id)
        {
            var result = await _productCategoryRepository.GetById(id);
            return Ok(result);
        }
    }
}
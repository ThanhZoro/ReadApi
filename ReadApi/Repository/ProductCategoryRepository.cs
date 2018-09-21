using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Nest;
using Newtonsoft.Json;
using ReadApi.Data;
using QueryFailOverEsMongo.Models;

namespace ReadApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        private ElasticClient _esClient;
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="esSettings"></param>
        /// <param name="applicationDbContext"></param>
        /// <param name="httpContextAccessor"></param>
        public ProductCategoryRepository(
            IOptions<ElasticSearchSettings> esSettings,
            ApplicationDbContext applicationDbContext,
            IHttpContextAccessor httpContextAccessor)
        {
            var node = new Uri($"http://{esSettings.Value.Host}:{esSettings.Value.Port}");
            var connSettings = new ConnectionSettings(node);
            connSettings.DefaultIndex("product_categories");
            connSettings.DefaultTypeName("productcategory");
            _esClient = new ElasticClient(connSettings);
            _httpContextAccessor = httpContextAccessor;
            _dbContext = applicationDbContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ProductCategory> GetById(string id)
        {
            var roles = _httpContextAccessor.HttpContext.GetRouteValue("roles")?.ToString().Split(",");
            var userName = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(s => s.Type == "userName")?.Value;
            var companyId = _httpContextAccessor.HttpContext.Request?.Headers["CompanyId"].FirstOrDefault();
            QueryContainer accessRightContainer = new QueryContainer();
            if (!roles.Contains("COMPANY_DATA"))
            {
                accessRightContainer = Query<ProductCategory>.Term(t => t.CreatedBy, userName);
            }
            var searchResponse = await _esClient.SearchAsync<ProductCategory>(s => s
                        .Size(1)
                        .Query(q => q.Term(t => t.Id, id) && q.Term(t => t.CompanyId, companyId) && q.Term(t => t.IsDelete, false) && accessRightContainer)
                    );
            return searchResponse?.Documents?.FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<DatasourceResult<List<ProductCategory>>> GetByQuery(ElasticSearchQuery query)
        {
            var roles = _httpContextAccessor.HttpContext.GetRouteValue("roles")?.ToString().Split(",");
            var userName = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(s => s.Type == "userName")?.Value;
            var companyId = _httpContextAccessor.HttpContext.Request?.Headers["CompanyId"].FirstOrDefault();
            var listproductCategories = new DatasourceResult<List<ProductCategory>>
            {
                From = query.From,
                Size = query.Size
            };
            QueryContainer accessRightContainer = new QueryContainer();
            if (!roles.Contains("COMPANY_DATA"))
            {
                accessRightContainer = Query<ProductCategory>.Term(t => t.CreatedBy, userName);
            }
            var searchResponse = await _esClient.SearchAsync<ProductCategory>(s => s
                    .From(query.From)
                    .Size(query.Size)
                    .Sort(ss => ss.Field(query.Sort.Field, (SortOrder)query.Sort.SortOrder))
                    .Source(so => so
                        .Includes(i => i.Fields(query.Source.Includes.ToArray()))
                        .Excludes(e => e.Fields(query.Source.Excludes.ToArray())))
                    .Query(q => q
                            .Raw(JsonConvert.SerializeObject(query.Query)) && q.Term(t => t.CompanyId, companyId) && q.Term(t => t.IsDelete, false) && accessRightContainer)
                );
            listproductCategories.Total = searchResponse.Total;
            listproductCategories.Data = searchResponse.Documents.ToList();
            return listproductCategories;
        }
    }
}

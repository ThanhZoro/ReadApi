using Contracts.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Nest;
using ReadApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReadApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class CommonDataRepository : ICommonDataRepository
    {
        private ElasticClient _esClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="esSettings"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="applicationDbContext"></param>
        public CommonDataRepository(IOptions<ElasticSearchSettings> esSettings, IHttpContextAccessor httpContextAccessor, ApplicationDbContext applicationDbContext)
        {
            var node = new Uri($"http://{esSettings.Value.Host}:{esSettings.Value.Port}");
            var connSettings = new ConnectionSettings(node);
            connSettings.DefaultIndex("common_data");
            connSettings.DefaultTypeName("commondata");
            _esClient = new ElasticClient(connSettings);
            _httpContextAccessor = httpContextAccessor;
            _dbContext = applicationDbContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<CommonData>> GetAll()
        {
            var companyId = _httpContextAccessor.HttpContext.Request?.Headers["CompanyId"].FirstOrDefault();
            var searchResponse = await _esClient.SearchAsync<CommonData>(s => s
                        .From(0)
                        .Size(5000)
                        .Sort(ss => ss.Field(f => f.CreatedAt, SortOrder.Descending))
                        .Query(q => q.Term(f => f.CompanyId, companyId))
                    );

            return searchResponse.Documents.ToList();
        }
    }
}

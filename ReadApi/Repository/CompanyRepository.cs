using System;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Nest;
using ReadApi.Data;

namespace ReadApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class CompanyRepository : ICompanyRepository
    {
        private ElasticClient _esClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="esSettings"></param>
        /// <param name="httpContextAccessor"></param>
        public CompanyRepository(IOptions<ElasticSearchSettings> esSettings, IHttpContextAccessor httpContextAccessor)
        {
            var node = new Uri($"http://{esSettings.Value.Host}:{esSettings.Value.Port}");
            var connSettings = new ConnectionSettings(node);
            connSettings.DefaultIndex("companies");
            connSettings.DefaultTypeName("company");
            _esClient = new ElasticClient(connSettings);
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<Company> GetById()
        {
            var companyId = _httpContextAccessor.HttpContext.Request?.Headers["CompanyId"].FirstOrDefault();
            var result = await _esClient.GetAsync<Company>(companyId);
            return result.Source;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<string> GetIdByCode(string code)
        {
            var companyId = _httpContextAccessor.HttpContext.Request?.Headers["CompanyId"].FirstOrDefault();
            var searchResponse = await _esClient.SearchAsync<Company>(s => s
                        .Size(1)
                        .Query(q => q.Term(t => t.CompanyCode, code))
                    );
            return searchResponse?.Documents?.FirstOrDefault()?.Id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Company> GetByExistId(string id)
        {
            var result = await _esClient.GetAsync<Company>(id);
            return result.Source;
        }
    }
}

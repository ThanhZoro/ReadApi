using Contracts.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Nest;
using Newtonsoft.Json;
using ReadApi.Data;
using QueryFailOverEsMongo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReadApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class ChatLeadRepository : IChatLeadRepository
    {
        private ElasticClient _esClient;
        private ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="esSettings"></param>
        /// <param name="applicationDbContext"></param>
        /// <param name="httpContextAccessor"></param>
        public ChatLeadRepository(
            IOptions<ElasticSearchSettings> esSettings, 
            ApplicationDbContext applicationDbContext, 
            IHttpContextAccessor httpContextAccessor)
        {
            var node = new Uri($"http://{esSettings.Value.Host}:{esSettings.Value.Port}");
            var connSettings = new ConnectionSettings(node);
            connSettings.DefaultIndex("chat_leads");
            connSettings.DefaultTypeName("chatlead");
            _esClient = new ElasticClient(connSettings);
            _dbContext = applicationDbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="leadId"></param>
        /// <returns></returns>
        public async Task<List<ChatLead>> Get(string leadId)
        {
            var companyId = _httpContextAccessor.HttpContext.Request?.Headers["CompanyId"].FirstOrDefault();
            var searchResponse = await _esClient.SearchAsync<ChatLead>(s => s
                        .From(0)
                        .Size(5000)
                        .Sort(ss => ss.Field(f => f.CreatedAt, SortOrder.Descending))
                        .Query(q => q.Term(t => t.LeadId, leadId) && q.Term(t => t.CompanyId, companyId))
                    );

            return searchResponse?.Documents?.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<DatasourceResult<List<ChatLead>>> GetByQuery(ElasticSearchQuery query)
        {
            var companyId = _httpContextAccessor.HttpContext.Request?.Headers["CompanyId"].FirstOrDefault();
            var data = new DatasourceResult<List<ChatLead>>
            {
                From = query.From,
                Size = query.Size
            };
            var searchResponse = await _esClient.SearchAsync<ChatLead>(s => s
                        .From(query.From)
                        .Size(query.Size)
                        .Sort(ss => ss.Field(query.Sort.Field, (SortOrder)query.Sort.SortOrder))
                        .Source(so => so
                            .Includes(i => i.Fields(query.Source.Includes.ToArray()))
                            .Excludes(e => e.Fields(query.Source.Excludes.ToArray())))
                        .Query(q => q
                                .Raw(JsonConvert.SerializeObject(query.Query)) && q.Term(t => t.CompanyId, companyId))
                    );

            data.Total = searchResponse.Total;
            data.Data = searchResponse.Documents.ToList();

            return data;
        }
    }
}

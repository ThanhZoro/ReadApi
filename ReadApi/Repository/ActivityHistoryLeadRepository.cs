using Contracts.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Nest;
using Newtonsoft.Json;
using ReadApi.Data;
using ReadApi.Models;
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
    public class ActivityHistoryLeadRepository : IActivityHistoryLeadRepository
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
        public ActivityHistoryLeadRepository(IOptions<ElasticSearchSettings> esSettings, ApplicationDbContext applicationDbContext, IHttpContextAccessor httpContextAccessor)
        {
            var node = new Uri($"http://{esSettings.Value.Host}:{esSettings.Value.Port}");
            var connSettings = new ConnectionSettings(node);
            connSettings.DefaultIndex("activity_history_leads");
            connSettings.DefaultTypeName("activityhistorylead");
            _esClient = new ElasticClient(connSettings);
            _dbContext = applicationDbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<List<ActivityHistoryLead>> Get(GetActivityHistoryLead data)
        {
            var companyId = _httpContextAccessor.HttpContext.Request?.Headers["CompanyId"].FirstOrDefault();
            var searchResponse = await _esClient.SearchAsync<ActivityHistoryLead>(s => s
                        .From(0)
                        .Size(5000)
                        .Sort(ss => ss.Field(f => f.CreatedAt, SortOrder.Descending))
                        .Query(q => q.Term(t => t.LeadId, data.LeadId) && q.Terms(t => t.Field(f => f.Type).Terms(data.Type)) && q.Term(t => t.CompanyId, companyId))
                    );

            return searchResponse?.Documents?.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<DatasourceResult<List<ActivityHistoryLead>>> GetByQuery(ElasticSearchQuery query)
        {
            var companyId = _httpContextAccessor.HttpContext.Request?.Headers["CompanyId"].FirstOrDefault();
            var data = new DatasourceResult<List<ActivityHistoryLead>>
            {
                From = query.From,
                Size = query.Size
            };
            var searchResponse = await _esClient.SearchAsync<ActivityHistoryLead>(s => s
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Nest;
using ReadApi.Data;

namespace ReadApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class ReportRepository : IReportRepository
    {
        private ElasticClient _esClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="esSettings"></param>
        /// <param name="httpContextAccessor"></param>
        public ReportRepository(IOptions<ElasticSearchSettings> esSettings, IHttpContextAccessor httpContextAccessor)
        {
            var node = new Uri($"http://{esSettings.Value.Host}:{esSettings.Value.Port}");
            var connSettings = new ConnectionSettings(node);
            connSettings.DefaultIndex("leads");
            connSettings.DefaultTypeName("lead");
            _esClient = new ElasticClient(connSettings);
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<string, Dictionary<string, long?>>> LeadReport()
        {
            var roles = _httpContextAccessor.HttpContext.GetRouteValue("roles")?.ToString().Split(",");
            var teams = _httpContextAccessor.HttpContext.GetRouteValue("teams")?.ToString().Split(",");
            var companyId = _httpContextAccessor.HttpContext.Request?.Headers["CompanyId"].FirstOrDefault();
            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(s => s.Type == "sub")?.Value;

            QueryContainer accessRightContainer = new QueryContainer();

            if (!roles.Contains("COMPANY_DATA"))
            {
                if (roles.Contains("TEAM_DATA"))
                {
                    accessRightContainer = Query<Lead>.Terms(t => t.Field(f => f.TeamId).Terms(teams)) || Query<Lead>.Term(t => t.StaffInCharge, userId) || Query<Lead>.Term(t => t.SupportStaff, userId);
                }
                else
                {
                    accessRightContainer = Query<Lead>.Term(t => t.StaffInCharge, userId) || Query<Lead>.Term(t => t.SupportStaff, userId);
                }
            }

            var data = new Dictionary<string, Dictionary<string, long?>>();
            var aggLead = await _esClient.SearchAsync<Lead>(s => s.Query(q => q.Term(t => t.CompanyId, companyId) && q.Term(t => t.IsDelete, false) && accessRightContainer)
                .Aggregations(a => a
                    .Terms("status_aggs", st => st.Field("status.keyword")) && a.Terms("source_aggs", st => st.Field("source.keyword")) && a.Terms("channel_aggs", st => st.Field("channel.keyword"))
                    )
                .Size(0)
            );



            data.Add("status_aggs", aggLead.Aggregations.Terms("status_aggs").Buckets.ToDictionary(mc => mc.Key, mc => mc.DocCount));
            data.Add("source_aggs", aggLead.Aggregations.Terms("source_aggs").Buckets.ToDictionary(mc => mc.Key, mc => mc.DocCount));
            data.Add("channel_aggs", aggLead.Aggregations.Terms("channel_aggs").Buckets.ToDictionary(mc => mc.Key, mc => mc.DocCount));
            return data;
        }
    }
}

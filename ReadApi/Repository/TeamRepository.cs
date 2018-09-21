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
    public class TeamRepository : ITeamRepository
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
        public TeamRepository(
            IOptions<ElasticSearchSettings> esSettings,
            ApplicationDbContext applicationDbContext,
            IHttpContextAccessor httpContextAccessor)
        {
            var node = new Uri($"http://{esSettings.Value.Host}:{esSettings.Value.Port}");
            var connSettings = new ConnectionSettings(node);
            connSettings.DefaultIndex("teams");
            connSettings.DefaultTypeName("team");
            _esClient = new ElasticClient(connSettings);
            _httpContextAccessor = httpContextAccessor;
            _dbContext = applicationDbContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Team> GetById(string id)
        {
            var companyId = _httpContextAccessor.HttpContext.Request?.Headers["CompanyId"].FirstOrDefault();
            var searchResponse = await _esClient.SearchAsync<Team>(s => s
                        .Size(1)
                        .Query(q => q.Term(t => t.Id, id) && q.Term(t => t.CompanyId, companyId) && q.Term(t => t.IsDelete, false))
                    );
            return searchResponse?.Documents?.FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<DatasourceResult<List<Team>>> GetByQuery(ElasticSearchQuery query)
        {
            var companyId = _httpContextAccessor.HttpContext.Request?.Headers["CompanyId"].FirstOrDefault();
            var listTeams = new DatasourceResult<List<Team>>
            {
                From = query.From,
                Size = query.Size
            };
            var searchResponse = await _esClient.SearchAsync<Team>(s => s
                    .From(query.From)
                    .Size(query.Size)
                    .Sort(ss => ss.Field(query.Sort.Field, (SortOrder)query.Sort.SortOrder))
                    .Source(so => so
                            .Includes(i => i.Fields(query.Source.Includes.ToArray()))
                            .Excludes(e => e.Fields(query.Source.Excludes.ToArray())))
                    .Query(q => q
                            .Raw(JsonConvert.SerializeObject(query.Query)) && q.Term(t => t.CompanyId, companyId) && q.Term(t => t.IsDelete, false))
                );
            listTeams.Total = searchResponse.Total;
            listTeams.Data = searchResponse.Documents.ToList();
            return listTeams;
        }
    }
}

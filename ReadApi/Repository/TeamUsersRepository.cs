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
    public class TeamUsersRepository : ITeamUsersRepository
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
        public TeamUsersRepository(
            IOptions<ElasticSearchSettings> esSettings,
            ApplicationDbContext applicationDbContext,
            IHttpContextAccessor httpContextAccessor)
        {
            var node = new Uri($"http://{esSettings.Value.Host}:{esSettings.Value.Port}");
            var connSettings = new ConnectionSettings(node);
            connSettings.DefaultIndex("team_users");
            connSettings.DefaultTypeName("teamusers");
            _esClient = new ElasticClient(connSettings);
            _httpContextAccessor = httpContextAccessor;
            _dbContext = applicationDbContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<DatasourceResult<List<TeamUsers>>> GetByQuery(ElasticSearchQuery query)
        {
            var companyId = _httpContextAccessor.HttpContext.Request?.Headers["CompanyId"].FirstOrDefault();
            var listTeamUsers = new DatasourceResult<List<TeamUsers>>
            {
                From = query.From,
                Size = query.Size
            };
            var searchResponse = await _esClient.SearchAsync<TeamUsers>(s => s
                    .From(query.From)
                    .Size(query.Size)
                    .Sort(ss => ss.Field(query.Sort.Field, (SortOrder)query.Sort.SortOrder))
                    .Source(so => so
                            .Includes(i => i.Fields(query.Source.Includes.ToArray()))
                            .Excludes(e => e.Fields(query.Source.Excludes.ToArray())))
                    .Query(q => q
                            .Raw(JsonConvert.SerializeObject(query.Query)) && q.Term(t => t.CompanyId, companyId))
                );
            listTeamUsers.Total = searchResponse.Total;
            listTeamUsers.Data = searchResponse.Documents.ToList();
            return listTeamUsers;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TeamUsers> GetById(string id)
        {
            var companyId = _httpContextAccessor.HttpContext.Request?.Headers["CompanyId"].FirstOrDefault();
            var searchResponse = await _esClient.SearchAsync<TeamUsers>(s => s
                        .Size(1)
                        .Query(q => q.Term(t => t.Id, id) && q.Term(t => t.CompanyId, companyId))
                    );
            return searchResponse?.Documents?.FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<string>> GetTeamIds(string companyId, string userId)
        {
            var searchResponse = await _esClient.SearchAsync<TeamUsers>(s => s
                            .Size(1)
                            .Query(q => q.Term(t => t.CompanyId, companyId) && q.Term(t => t.UserId, userId))
                        );

            return searchResponse.Documents?.FirstOrDefault()?.TeamIds ?? new List<string>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<TeamUsers>> GetAll()
        {
            var companyId = _httpContextAccessor.HttpContext.Request?.Headers["CompanyId"].FirstOrDefault();
            var searchResponse = await _esClient.SearchAsync<TeamUsers>(s => s
                        .From(0)
                        .Size(5000)
                        .Sort(ss => ss.Field(f => f.CreatedAt, SortOrder.Descending))
                        .Query(q => q.Term(f => f.CompanyId, companyId))
                    );

            return searchResponse.Documents.ToList();
        }
    }
}

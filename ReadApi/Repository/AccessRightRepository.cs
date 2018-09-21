using Contracts.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Nest;
using ReadApi.Data;
using ReadApi.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReadApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class AccessRightRepository : IAccessRightRepository
    {
        private ElasticClient _esClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="esSettings"></param>
        /// <param name="httpContextAccessor"></param>
        public AccessRightRepository(IOptions<ElasticSearchSettings> esSettings, IHttpContextAccessor httpContextAccessor)
        {
            var node = new Uri($"http://{esSettings.Value.Host}:{esSettings.Value.Port}");
            var connSettings = new ConnectionSettings(node);
            connSettings.DefaultIndex("access_rights");
            _esClient = new ElasticClient(connSettings);
            _httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<UserRoles>> AllRoles(string companyId)
        {
            var searchResponse = await _esClient.SearchAsync<AccessRight>(s => s
                        .Size(100)
                        .Query(q => q.Term(t => t.CompanyId, companyId)));
            var data = searchResponse.Documents?.Select(s => new UserRoles() { UserId = s.UserId, Roles = s.RoleList }).ToList();
            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<string>> UserRoles(string companyId, string userId)
        {
            var data = new List<string>();
            var searchResponse = await _esClient.SearchAsync<AccessRight>(s => s
                        .Size(1)
                        .Query(q => q.Term(t => t.CompanyId, companyId) && q.Term(t => t.UserId, userId)));
            var userRoles = searchResponse.Documents?.FirstOrDefault();
            if (userRoles != null && userRoles.RoleList != null && userRoles.RoleList.Count > 0)
            {
                data = new RolesList().GetListSort().Array(userRoles.RoleList);
            }
            return data;
        }
    }
}

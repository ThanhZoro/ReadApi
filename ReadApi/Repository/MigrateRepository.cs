using Contracts.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Nest;
using ReadApi.Data;
using ReadApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReadApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class MigrateRepository : IMigrateRepository
    {
        private ElasticClient _esClient;
        private ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="esSettings"></param>
        /// <param name="appcontext"></param>
        public MigrateRepository(IOptions<ElasticSearchSettings> esSettings, ApplicationDbContext appcontext, IHttpContextAccessor httpContextAccessor)
        {
            var node = new Uri($"http://{esSettings.Value.Host}:{esSettings.Value.Port}");
            var connSettings = new ConnectionSettings(node);
            _esClient = new ElasticClient(connSettings);
            _context = appcontext;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<List<string>> DeleteES(DeleteES data)
        {
            var userName = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(s => s.Type == "userName")?.Value;
            if (userName == "vuongnd@twin.vn")
            {
                var response = await _esClient.DeleteManyAsync(data.Ids.Select(x => new Item { Id = x }), data.Index, data.Type);
            }
            return data.Ids;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<string> DeleteIndexES(string index)
        {
            var userName = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(s => s.Type == "userName")?.Value;
            if (userName == "vuongnd@twin.vn")
            {
                await _esClient.DeleteIndexAsync(index);
            }

            return index;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public async Task<long> Migrate(string index, string companyId)
        {
            IEnumerable<object> data = null;

            var userName = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(s => s.Type == "userName")?.Value;
            if (userName == "vuongnd@twin.vn")
            {
                if (!string.IsNullOrEmpty(companyId))
                {
                    switch (index)
                    {
                        case "activity_history_leads":
                            data = await _context.ActivityHistoryLead.Find(f => f.Id == companyId).ToListAsync();
                            break;
                        case "chat_leads":
                            var leads = await _context.Lead.Find(f => f.CompanyId == companyId).ToListAsync();
                            data = await _context.ChatLead.Find(f => leads.Any(a => a.Id == f.LeadId)).ToListAsync();
                            break;
                        case "companies":
                            data = await _context.Company.Find(f => f.Id == companyId).ToListAsync();
                            break;
                        case "contact_leads":
                            data = await _context.ContactLead.Find(f => f.CompanyId == companyId).ToListAsync();
                            break;
                        case "common_data":
                            data = await _context.CommonData.Find(f => f.CompanyId == companyId).ToListAsync();
                            break;
                        case "leads":
                            data = await _context.Lead.Find(f => f.CompanyId == companyId).ToListAsync();
                            break;
                        case "product_categories":
                            data = await _context.ProductCategory.Find(f => f.CompanyId == companyId).ToListAsync();
                            break;
                        case "products":
                            data = await _context.Product.Find(f => f.CompanyId == companyId).ToListAsync();
                            break;
                        case "teams":
                            data = await _context.Team.Find(f => f.CompanyId == companyId).ToListAsync();
                            break;
                        case "team_users":
                            data = await _context.TeamUsers.Find(f => f.CompanyId == companyId).ToListAsync();
                            break;
                        case "access_rights":
                            data = await _context.AccessRight.Find(f => f.CompanyId == companyId).ToListAsync();
                            break;
                    }
                }
                else
                {
                    switch (index)
                    {
                        case "activity_history_leads":
                            data = await _context.ActivityHistoryLead.Find(_ => true).ToListAsync();
                            break;
                        case "chat_leads":
                            data = await _context.ChatLead.Find(_ => true).ToListAsync();
                            break;
                        case "companies":
                            data = await _context.Company.Find(_ => true).ToListAsync();
                            break;
                        case "contact_leads":
                            data = await _context.ContactLead.Find(_ => true).ToListAsync();
                            break;
                        case "common_data":
                            data = await _context.CommonData.Find(_ => true).ToListAsync();
                            break;
                        case "leads":
                            data = await _context.Lead.Find(_ => true).ToListAsync();
                            break;
                        case "product_categories":
                            data = await _context.ProductCategory.Find(_ => true).ToListAsync();
                            break;
                        case "products":
                            data = await _context.Product.Find(_ => true).ToListAsync();
                            break;
                        case "teams":
                            data = await _context.Team.Find(_ => true).ToListAsync();
                            break;
                        case "team_users":
                            data = await _context.TeamUsers.Find(_ => true).ToListAsync();
                            break;
                        case "access_rights":
                            data = await _context.AccessRight.Find(_ => true).ToListAsync();
                            break;
                    }
                }
                if (data != null && data.Any())
                {
                    int skip = 0;
                    int take = 20000;
                    var types = GetTypes();
                    do
                    {
                        var response = await _esClient.IndexManyAsync(data.Skip(skip).Take(take).ToList(), index, types[index]);
                        skip += take;
                    }
                    while (skip < data.Count());
                }
            }


            return data != null ? data.Count() : 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task Reindex(bool refreshAllIndex, string refreshIndex, string companyId)
        {
            var userName = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(s => s.Type == "userName")?.Value;
            if (userName == "vuongnd@twin.vn")
            {
                if (refreshAllIndex)
                {
                    foreach (var item in GetTypes())
                    {
                        await DeleteIndexES(item.Key);
                    }
                }

                if (!string.IsNullOrEmpty(refreshIndex))
                {
                    await DeleteIndexES(refreshIndex);
                }

                var analyzers = new AnalysisDescriptor().Analyzers(aa => aa
                                    .Custom("folding", sa => sa.Tokenizer("standard").Filters("lowercase", "asciifolding").CharFilters("html_strip"))
                                    .Custom("sortable", sa => sa.Tokenizer("keyword").Filters("lowercase", "asciifolding"))
                                    .Custom("uax", sa => sa.Tokenizer("uax_url_email").Filters("lowercase", "stop"))
                            );

                var mappingDescriptor = new MappingsDescriptor();

                foreach (var item in GetTypes())
                {
                    if (!_esClient.IndexExists(item.Key).Exists)
                    {
                        switch (item.Key)
                        {
                            case "activity_history_leads":
                                mappingDescriptor = mappingDescriptor.Map<ActivityHistoryLead>(mm => mm
                                    .Properties(p => p
                                        .Text(t => t.Name(n => n.CreatedBy).Analyzer("uax").SearchAnalyzer("uax"))
                                    )
                                );
                                break;
                            case "chat_leads":
                                mappingDescriptor = mappingDescriptor.Map<ChatLead>(mm => mm
                                    .Properties(p => p
                                        .Text(t => t.Name(n => n.CreatedBy).Analyzer("uax").SearchAnalyzer("uax"))
                                    )
                                );
                                break;
                            case "companies":
                                mappingDescriptor = mappingDescriptor.Map<Company>(mm => mm
                                    .Properties(p => p
                                        .Text(t => t.Name(n => n.CompanyName).Analyzer("folding").SearchAnalyzer("folding"))
                                        .Text(t => t.Name(n => n.Email).Analyzer("uax").SearchAnalyzer("uax"))
                                        .Text(t => t.Name(n => n.CreatedBy).Analyzer("uax").SearchAnalyzer("uax"))
                                        .Text(t => t.Name(n => n.UpdatedBy).Analyzer("uax").SearchAnalyzer("uax"))
                                    )
                                );
                                break;
                            case "contact_leads":
                                mappingDescriptor = mappingDescriptor.Map<ContactLead>(mm => mm
                                    .Properties(p => p
                                        .Text(t => t.Name(n => n.Name).Analyzer("folding").SearchAnalyzer("folding"))
                                        .Text(t => t.Name(n => n.Email).Analyzer("uax").SearchAnalyzer("uax"))
                                        .Text(t => t.Name(n => n.CreatedBy).Analyzer("uax").SearchAnalyzer("uax"))
                                        .Text(t => t.Name(n => n.UpdatedBy).Analyzer("uax").SearchAnalyzer("uax"))
                                    )
                                );
                                break;
                            case "common_data":
                                mappingDescriptor = mappingDescriptor.Map<CommonData>(mm => mm
                                    .Properties(p => p
                                        .Text(t => t.Name(n => n.CreatedBy).Analyzer("uax").SearchAnalyzer("uax"))
                                        .Text(t => t.Name(n => n.UpdatedBy).Analyzer("uax").SearchAnalyzer("uax"))
                                    )
                                );
                                break;
                            case "leads":
                                mappingDescriptor = mappingDescriptor.Map<Lead>(mm => mm
                                    .Properties(p => p
                                        .Text(t => t.Name(n => n.FullName).Analyzer("folding").SearchAnalyzer("folding"))
                                        .Text(t => t.Name(n => n.Email).Analyzer("uax").SearchAnalyzer("uax"))
                                        .Text(t => t.Name(n => n.CreatedBy).Analyzer("uax").SearchAnalyzer("uax"))
                                        .Text(t => t.Name(n => n.UpdatedBy).Analyzer("uax").SearchAnalyzer("uax"))
                                    )
                                );
                                break;
                            case "product_categories":
                                mappingDescriptor = mappingDescriptor.Map<ProductCategory>(mm => mm
                                    .Properties(p => p
                                        .Text(t => t.Name(n => n.Name).Analyzer("folding").SearchAnalyzer("folding"))
                                        .Text(t => t.Name(n => n.CreatedBy).Analyzer("uax").SearchAnalyzer("uax"))
                                        .Text(t => t.Name(n => n.UpdatedBy).Analyzer("uax").SearchAnalyzer("uax"))
                                    )
                                );
                                break;
                            case "products":
                                mappingDescriptor = mappingDescriptor.Map<Product>(mm => mm
                                    .Properties(p => p
                                        .Text(t => t.Name(n => n.Name).Analyzer("folding").SearchAnalyzer("folding"))
                                        .Text(t => t.Name(n => n.CreatedBy).Analyzer("uax").SearchAnalyzer("uax"))
                                        .Text(t => t.Name(n => n.UpdatedBy).Analyzer("uax").SearchAnalyzer("uax"))
                                    )
                                );
                                break;
                            case "teams":
                                mappingDescriptor = mappingDescriptor.Map<Team>(mm => mm
                                    .Properties(p => p
                                        .Text(t => t.Name(n => n.Name).Analyzer("folding").SearchAnalyzer("folding"))
                                        .Text(t => t.Name(n => n.CreatedBy).Analyzer("uax").SearchAnalyzer("uax"))
                                        .Text(t => t.Name(n => n.UpdatedBy).Analyzer("uax").SearchAnalyzer("uax"))
                                    )
                                );
                                break;
                            case "team_users":
                                mappingDescriptor = mappingDescriptor.Map<TeamUsers>(mm => mm
                                    .Properties(p => p
                                        .Text(t => t.Name(n => n.CreatedBy).Analyzer("uax").SearchAnalyzer("uax"))
                                        .Text(t => t.Name(n => n.UpdatedBy).Analyzer("uax").SearchAnalyzer("uax"))
                                    )
                                );
                                break;
                            case "access_rights":
                                mappingDescriptor = mappingDescriptor.Map<AccessRight>(mm => mm
                                    .Properties(p => p
                                       .Text(t => t.Name(n => n.CreatedBy).Analyzer("uax").SearchAnalyzer("uax"))
                                        .Text(t => t.Name(n => n.UpdatedBy).Analyzer("uax").SearchAnalyzer("uax"))
                                    )
                                );
                                break;
                        }
                        await _esClient.CreateIndexAsync(item.Key, c => c
                            .Settings(s => s.Analysis(a => analyzers))
                            .Mappings(m => mappingDescriptor
                            )
                        );

                        await Migrate(item.Key, companyId);
                    }
                }
            }
        }

        private Dictionary<string, string> GetTypes()
        {
            return new Dictionary<string, string>
            {
                { "common_data", "commondata"},
                { "companies", "company" },
                { "activity_history_leads", "activityhistorylead" },
                { "chat_leads", "chatlead" },
                { "contact_leads", "contactlead" },
                { "leads", "lead" },
                { "product_categories", "productcategory" },
                { "products", "product" },
                { "teams", "team" },
                { "team_users", "teamusers" },
                { "access_rights", "accessright" },
            };
        }
    }
}
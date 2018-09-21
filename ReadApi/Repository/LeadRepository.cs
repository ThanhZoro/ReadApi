using Contracts.Commands;
using Contracts.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Nest;
using Newtonsoft.Json;
using ReadApi.Data;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QueryFailOverEsMongo.Common;
using QueryFailOverEsMongo.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ReadApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class LeadRepository : ILeadRepository
    {
        private ElasticClient _esClient;
        private readonly ICommonDataRepository _commonDataRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="esSettings"></param>
        /// <param name="commonDataRepository"></param>
        /// <param name="accountRepository"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="applicationDbContext"></param>
        public LeadRepository(
            IOptions<ElasticSearchSettings> esSettings,
            ICommonDataRepository commonDataRepository,
            IAccountRepository accountRepository,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext applicationDbContext)
        {
            var node = new Uri($"http://{esSettings.Value.Host}:{esSettings.Value.Port}");
            var connSettings = new ConnectionSettings(node);
            connSettings.DefaultIndex("leads");
            _esClient = new ElasticClient(connSettings);
            _commonDataRepository = commonDataRepository;
            _accountRepository = accountRepository;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = applicationDbContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Lead> GetById(string id)
        {
            var result = await _esClient.GetAsync<Lead>(id);
            return result.Source;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<DatasourceResult<List<Lead>>> GetByQuery(ElasticSearchQuery query)
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

            var result = new DatasourceResult<List<Lead>>
            {
                From = query.From,
                Size = query.Size
            };
            var searchResponse = await _esClient.SearchAsync<Lead>(s => s
                       .From(query.From)
                       .Size(query.Size)
                       .Sort(ss => ss.Field(query.Sort.Field, (SortOrder)query.Sort.SortOrder))
                       .Source(so => so
                            .Includes(i => i.Fields(query.Source.Includes.ToArray()))
                            .Excludes(e => e.Fields(query.Source.Excludes.ToArray())))
                       .Query(q => q
                               .Raw(JsonConvert.SerializeObject(query.Query)) && q.Term(t => t.CompanyId, companyId) && q.Term(t => t.IsDelete, false) && accessRightContainer)
                   );

            result.Total = searchResponse.Total;
            result.Data = searchResponse.Documents.ToList();
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="language"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<string> Export(ElasticSearchQuery query, string language, string userId)
        {
            query.From = 0;
            query.Size = QueryConstant.MaxLimit;
            var listLeads = await GetByQuery(query);
            var leads = listLeads.Data;
            var users = await _accountRepository.GetAll();

            var commonData = await _commonDataRepository.GetAll();
            var leadStatus = commonData.Where(w => w.DataType == CommonDataType.Status);
            var definitionSources = commonData.Where(w => w.DataType == CommonDataType.Source);
            var definitionChannels = commonData.Where(w => w.DataType == CommonDataType.Channel);
            var listVocative = commonData.Where(w => w.DataType == CommonDataType.Vocative);
            var listMaritalStatus = commonData.Where(w => w.DataType == CommonDataType.MaritalStatus);
            var listRelationship = commonData.Where(w => w.DataType == CommonDataType.Relationship);
            var listGender = commonData.Where(w => w.DataType == CommonDataType.Gender);

            string excelName = $"lead_export_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
            var path = Path.Combine(
                           Directory.GetCurrentDirectory(),
                           "wwwroot/Export", excelName);
            FileInfo newFile = new FileInfo(path);
            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Lead");
            workSheet.TabColor = Color.Black;
            workSheet.DefaultRowHeight = 12;

            //Header of table
            if (language == "vi")
            {
                workSheet.Row(1).Height = 20;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.Font.Bold = true;
                workSheet.Cells[1, 1].Value = "Họ tên";
                workSheet.Cells[1, 2].Value = "Điện thoại";
                workSheet.Cells[1, 3].Value = "Email";
                workSheet.Cells[1, 4].Value = "Social Id";
                workSheet.Cells[1, 5].Value = "Người phụ trách";
                workSheet.Cells[1, 6].Value = "Người hỗ trợ";
                workSheet.Cells[1, 7].Value = "Mối quan hệ";
                workSheet.Cells[1, 8].Value = "Xưng hô";
                workSheet.Cells[1, 9].Value = "Giới tính";
                workSheet.Cells[1, 10].Value = "Ngày sinh";
                workSheet.Cells[1, 11].Value = "Tình trạng hôn nhân";
                workSheet.Cells[1, 12].Value = "Trạng thái";
                workSheet.Cells[1, 13].Value = "Sản phẩm quan tâm";
                workSheet.Cells[1, 14].Value = "Ghi chú";
                workSheet.Cells[1, 15].Value = "Nguồn";
                workSheet.Cells[1, 16].Value = "Kênh";
                workSheet.Cells[1, 17].Value = "Chiến dịch";
                workSheet.Cells[1, 18].Value = "Địa chỉ";
                workSheet.Cells[1, 19].Value = "Số CMND";
                workSheet.Cells[1, 20].Value = "Ngày cấp";
                workSheet.Cells[1, 21].Value = "Nơi cấp";
            }
            else
            {
                workSheet.Row(1).Height = 20;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.Font.Bold = true;
                workSheet.Cells[1, 1].Value = "Fullname";
                workSheet.Cells[1, 2].Value = "Phone";
                workSheet.Cells[1, 3].Value = "Email";
                workSheet.Cells[1, 4].Value = "Social Id";
                workSheet.Cells[1, 5].Value = "Staff in charge";
                workSheet.Cells[1, 6].Value = "Support";
                workSheet.Cells[1, 7].Value = "Relationship";
                workSheet.Cells[1, 8].Value = "Title";
                workSheet.Cells[1, 9].Value = "Gender";
                workSheet.Cells[1, 10].Value = "Birthday";
                workSheet.Cells[1, 11].Value = "Marriage";
                workSheet.Cells[1, 12].Value = "Lead care status";
                workSheet.Cells[1, 13].Value = "Product interesting";
                workSheet.Cells[1, 14].Value = "Note";
                workSheet.Cells[1, 15].Value = "Source";
                workSheet.Cells[1, 16].Value = "Source channel";
                workSheet.Cells[1, 17].Value = "Campaign";
                workSheet.Cells[1, 18].Value = "Address";
                workSheet.Cells[1, 19].Value = "Identify";
                workSheet.Cells[1, 20].Value = "Date of issue";
                workSheet.Cells[1, 21].Value = "Place of issue";
            }

            //Body of table  
            int recordIndex = 2;
            foreach (var lead in leads)
            {
                var status = leadStatus.FirstOrDefault(f => f.Id == lead.Status);
                var source = definitionSources.FirstOrDefault(f => f.Id == lead.Source);
                var channel = definitionChannels.FirstOrDefault(f => f.Id == lead.Channel);
                var staffInCharge = users.FirstOrDefault(f => f.Id == lead.StaffInCharge);
                var supportStaff = string.Join(", ", users.Where(w => lead.SupportStaff.Contains(w.Id)).Distinct().Select(s => s.FirstName + " " + s.LastName).ToList());
                var vocative = listVocative.FirstOrDefault(f => f.Id == lead.Vocative);
                var maritalStatus = listMaritalStatus.FirstOrDefault(f => f.Id == lead.MaritalStatus);
                var relationship = listRelationship.FirstOrDefault(f => f.Id == lead.Relationship);
                var gender = listGender.FirstOrDefault(f => f.Id == lead.Gender);
                workSheet.Cells[recordIndex, 1].Value = lead.FullName;
                workSheet.Cells[recordIndex, 2].Value = lead.Phone;
                workSheet.Cells[recordIndex, 3].Value = lead.Email;
                workSheet.Cells[recordIndex, 4].Value = lead.SocialId;
                workSheet.Cells[recordIndex, 5].Value = staffInCharge != null ? $"{staffInCharge.FirstName} {staffInCharge.LastName}" : string.Empty;
                workSheet.Cells[recordIndex, 6].Value = supportStaff;
                workSheet.Cells[recordIndex, 7].Value = relationship != null ? relationship.DataValue : string.Empty;
                workSheet.Cells[recordIndex, 8].Value = vocative != null ? vocative.DataValue : string.Empty;
                workSheet.Cells[recordIndex, 9].Value = gender != null ? gender.DataValue : string.Empty;
                workSheet.Cells[recordIndex, 10].Value = lead.Birthday != null ? lead.Birthday.Value.ToString("MM/dd/yyyy") : "";
                workSheet.Cells[recordIndex, 11].Value = maritalStatus != null ? maritalStatus.DataValue : string.Empty;
                workSheet.Cells[recordIndex, 12].Value = status != null ? status.DataValue : string.Empty;
                workSheet.Cells[recordIndex, 13].Value = lead.Interest;
                workSheet.Cells[recordIndex, 14].Value = lead.Note;
                workSheet.Cells[recordIndex, 15].Value = source != null ? source.DataValue : string.Empty;
                workSheet.Cells[recordIndex, 16].Value = channel != null ? channel.DataValue : string.Empty;
                workSheet.Cells[recordIndex, 17].Value = lead.Campaign;
                workSheet.Cells[recordIndex, 18].Value = lead.Address;
                workSheet.Cells[recordIndex, 19].Value = lead.IdentityCard;
                workSheet.Cells[recordIndex, 20].Value = lead.DateOfIssue != null ? lead.DateOfIssue.Value.ToString("MM/dd/yyyy") : "";
                workSheet.Cells[recordIndex, 21].Value = lead.PlaceOfIssue;
                recordIndex++;
            }

            //auto fit
            //for (int i = 1; i <= 24; i++)
            //{
            //    workSheet.Column(i).AutoFit();
            //}
            //color header
            Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#1e88e5");
            workSheet.Cells["A1:U1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            workSheet.Cells["A1:U1"].Style.Fill.BackgroundColor.SetColor(colFromHex);
            workSheet.Cells["A1:U1"].Style.Font.Color.SetColor(Color.White);

            //border
            string modelRange = "A1:U" + (--recordIndex).ToString();
            var modelTable = workSheet.Cells[modelRange];
            modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            modelTable.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            modelTable.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            excel.SaveAs(newFile);

            return excelName;
        }
    }
}

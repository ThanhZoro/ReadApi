using Contracts.Commands;
using Contracts.Models;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using ReadApi.Repository;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Consumers
{
    /// <summary>
    /// 
    /// </summary>
    public class CheckAccessRightConsumer : IConsumer<ICheckAccessRight>
    {
        private IAccessRightRepository _accessRightRepository;
        private ICompanyRepository _companyRepository;
        private ITeamUsersRepository _teamUsersRepository;
        private readonly IDistributedCache _distributedCache;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessRightRepository"></param>
        /// <param name="companyRepository"></param>
        /// <param name="teamUsersRepository"></param>
        /// <param name="distributedCache"></param>
        public CheckAccessRightConsumer(IAccessRightRepository accessRightRepository, ICompanyRepository companyRepository, ITeamUsersRepository teamUsersRepository, IDistributedCache distributedCache)
        {
            _accessRightRepository = accessRightRepository;
            _companyRepository = companyRepository;
            _teamUsersRepository = teamUsersRepository;
            _distributedCache = distributedCache;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Consume(ConsumeContext<ICheckAccessRight> context)
        {
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.Console(theme: ConsoleTheme.None)
               .CreateLogger();
            var start = Stopwatch.GetTimestamp();
            Log.Information("Received command {CommandName}-{MessageId}: {@Messages}", GetType().Name, context.MessageId, context.Message);

            var data = context.Message;

            var responseData = new CheckAccessRightResponse();
            var cacheKey = $"access-right-{data.CompanyId}-{data.UserId}";

            var cacheValue = await _distributedCache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cacheValue))
            {
                responseData = JsonConvert.DeserializeObject<CheckAccessRightResponse>(cacheValue);
            }
            else
            {
                var company = await _companyRepository.GetByExistId(data.CompanyId);
                var userRoles = await _accessRightRepository.UserRoles(data.CompanyId, data.UserId);
                var teamIds = await _teamUsersRepository.GetTeamIds(data.CompanyId, data.UserId);

                var isAdmin = company.OwnerId == data.UserId;
                userRoles = isAdmin ? new RolesList().GetListSort().FirstOrDefault(f => f.Key == "Company").Value.FirstOrDefault(f => f.Key == "Company Owner").Value.ToList() : userRoles;
                responseData = new CheckAccessRightResponse
                {
                    HasAccess = ((userRoles.Intersect(data.RequestAccess.Split(",")).Count() > 0) || isAdmin),
                    RequestAccess = data.RequestAccess,
                    Roles = userRoles ?? new List<string>(),
                    IsAdmin = isAdmin,
                    Teams = teamIds ?? new List<string>()
                };

                if (responseData != null)
                {
                    await _distributedCache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(responseData), new DistributedCacheEntryOptions() { AbsoluteExpiration = DateTime.Now.AddMinutes(5) });
                }
            }

            await context.RespondAsync(responseData);
            Log.Information("Completed command {CommandName}-{MessageId} Response {@Response} in {ExecuteTime}ms", GetType().Name, context.MessageId, responseData, (Stopwatch.GetTimestamp() - start) * 1000 / (double)Stopwatch.Frequency);
        }
    }
}

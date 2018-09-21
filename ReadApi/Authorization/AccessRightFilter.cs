using Contracts.Models;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace TNTMgmt.Authorization
{
    /// <summary>
    /// 
    /// </summary>
    public class AccessRightAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessRight"></param>
        public AccessRightAttribute(string accessRight) : base(typeof(AccessRightFilter))
        {
            Arguments = new object[] { accessRight };
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class AccessRightFilter : IAsyncAuthorizationFilter
    {
        private readonly string _accessRight;
        private readonly IRequestClient<ICheckAccessRight, CheckAccessRightResponse> _checkAccessRightRequestClient;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessRight"></param>
        /// <param name="checkAccessRightRequestClient"></param>
        public AccessRightFilter(string accessRight, IRequestClient<ICheckAccessRight, CheckAccessRightResponse> checkAccessRightRequestClient)
        {
            _accessRight = accessRight;
            _checkAccessRightRequestClient = checkAccessRightRequestClient;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var companyId = context.HttpContext.Request.Headers["companyId"].ToString();
            var userId = context.HttpContext.User.Claims.Where(w => w.Type == "sub").FirstOrDefault()?.Value;

            var data = await _checkAccessRightRequestClient.Request(new { UserId = userId, CompanyId = companyId, RequestAccess = _accessRight });

            if (!data.HasAccess)
            {
                context.Result = new JsonResult(new { HttpStatusCode.Forbidden });
            }
            else
            {
                context.RouteData.Values.Add("roles", string.Join(",", data.Roles));
                context.RouteData.Values.Add("teams", string.Join(",", data.Teams));
            }
        }
    }
}

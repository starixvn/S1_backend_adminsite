using System.Security.Claims;

using BackendSite.Service.Model;

using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2
{
    public class BaseController : ControllerBase
    {
        public AuthData AuthData { get; set; }

        public BaseController(IHttpContextAccessor httpContextAccessor)
        {
            if (!httpContextAccessor.HttpContext.User.Identity.IsAuthenticated) return;

            int.TryParse(httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(m => m.Type == ClaimTypes.NameIdentifier)?.Value, out var userId);
            int.TryParse(httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(m => m.Type == ClaimTypes.Thumbprint)?.Value, out var siteId);

            AuthData = new AuthData
            {
                UserId = userId,
                UserName = httpContextAccessor.HttpContext.User.Identity.Name,
                SiteId = siteId
            };
        }
    }
}

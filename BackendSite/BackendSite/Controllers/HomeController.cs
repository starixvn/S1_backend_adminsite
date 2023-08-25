using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Ok();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return BadRequest();
        }

        [HttpGet]
        [AllowAnonymous]
        public ApiResult AuthorizedFailed()
        {
            return new ApiResult
            {
                ErrorCode = (int)ErrorCodeEnum.Fail,
                Message = "Unauthorized."
            };
        }
    }
}

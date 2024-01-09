using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa.WinLoss;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.ProductManagement
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class SlotGameGeneratorController : Controller
    {        
        public bool AddSlotGame(string request)
        {
            return true;
        }
    }
}

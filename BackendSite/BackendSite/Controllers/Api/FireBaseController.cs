using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BackendSite.Controllers.Api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FireBaseController
    {
        private readonly FireBaseBLL fireBaseBLL;

        public FireBaseController(FireBaseBLL fireBaseBLL)
        {
            this.fireBaseBLL = fireBaseBLL;
        }

        public class SendFireBaseMessageReq
        {
            public int FireBaseId { get; set; }
        }

        [HttpPost]
        public async Task<ApiResult> SendFireBaseMessageAsync(SendFireBaseMessageReq sendFireBaseMessageReq)
        {

            var apiResult = await fireBaseBLL.SendFireBaseMessageAsync(sendFireBaseMessageReq.FireBaseId);

            return apiResult;
        }
    }
}

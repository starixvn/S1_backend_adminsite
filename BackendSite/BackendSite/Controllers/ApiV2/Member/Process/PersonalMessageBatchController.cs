using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Controllers.ApiV2.Member.Process
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PersonalMessageBatchController : ControllerBase
    {

    }
}

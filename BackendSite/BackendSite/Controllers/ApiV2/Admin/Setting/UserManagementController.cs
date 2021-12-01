using BackendSite.Service.BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BackendSite.Controllers.ApiV2.Admin.Setting
{ 
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        #region Fields

        private readonly AdminUserBLL _adminUserBll;
        private readonly AdminGroupBLL _adminGroupBll;
        private readonly ServerInfoBLL _serverInfoBll;

        #endregion

        #region Constructors

        public UserManagementController(IServiceProvider provider)
        {
            _adminUserBll = provider.GetRequiredService<AdminUserBLL>();
            _adminGroupBll = provider.GetRequiredService<AdminGroupBLL>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
        }

        #endregion

        #region API Model

        #endregion

        #region Actions

        #endregion

    }
}

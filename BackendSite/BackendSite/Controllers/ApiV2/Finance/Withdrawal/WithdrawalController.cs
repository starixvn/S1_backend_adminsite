using System;
using System.Collections.Generic;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Finance.Withdrawal
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class WithdrawalController : ControllerBase
    {
        #region Fields

        private readonly WithdrawalBLL _withdrawalBll;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructors
        
        public WithdrawalController(IServiceProvider provider,IConfiguration configuration)
        {
            _withdrawalBll = provider.GetRequiredService<WithdrawalBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }
        
        #endregion
        
    }
}
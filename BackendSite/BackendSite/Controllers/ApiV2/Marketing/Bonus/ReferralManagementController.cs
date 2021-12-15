using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Marketing.Bonus
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/BonusManage" })]
    public class ReferralManagementController : ControllerBase
    {
        #region Fields
        
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly ReferralManagementBLL _referralManagementBll;
        private readonly ReportBLL _reportBll;
        
        #endregion

        #region Constructors

        public ReferralManagementController(IServiceProvider provider)
        {
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _referralManagementBll = provider.GetRequiredService<ReferralManagementBLL>();
            _reportBll = provider.GetRequiredService<ReportBLL>();
        }

        #endregion

        #region API Model

        public class GetReferralReq
        {
            public int SiteId { get; set; }
        }

        public class SetReferralStatusReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
        }

        public class SetReferralReq
        {
            public string Referral { get; set; }
        }

        #endregion
        
        #region API

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/ReferralManagement" })]
        public ApiResult<List<ReferralManagement>> GetReferral(GetReferralReq req)
        {
            var apiResult = new ApiResult<List<ReferralManagement>>();
            try
            {
                var siteList = _serverInfoBll.GetSiteListByLoginUser();
                var referralList = _referralManagementBll.GetReferralBySiteId(req.SiteId).ToList();
                var currencyList = _reportBll.GetCurrency().ToList();
                var referralManagementList = new List<ReferralManagement>();

                foreach (var currency in siteList.ToList().Find(x => x.SiteId == req.SiteId).CurrencyList.Replace("[", "").Replace("]", "").Split(","))
                {
                    int.TryParse(currency, out var currencyId);
                    var referral = referralList.FirstOrDefault(x => x.CurrencyId == currencyId);
                    referralManagementList.Add(new ReferralManagement
                    {
                        SiteId = req.SiteId,
                        CurrencyId = currencyId,
                        PeriodDays = referral == null ? string.Empty : referral.PeriodDays.ToString(),
                        BonusAmount = referral == null ? string.Empty : referral.BonusAmount.ToString(),
                        VaildBet = referral == null ? string.Empty : referral.VaildBet.ToString(),
                        Deposit = referral == null ? string.Empty : referral.Deposit.ToString(),
                        IsoCode = currencyList.Find(x => x.CurrencyId == currencyId).IsoCode,
                        Status = referral?.Status ?? false
                    });
                }

                apiResult.Data = referralManagementList;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                return apiResult;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
                return apiResult;
            }
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/ReferralManagement" })]
        public ApiResult<dynamic> SetReferralStatus(SetReferralStatusReq req)
        {
            var apiResult = new ApiResult<dynamic>();
            try
            {
                var referralList = _referralManagementBll.GetReferralBySiteId(req.SiteId).ToList();
                var referral = referralList.FirstOrDefault(x => x.CurrencyId == req.CurrencyId);
                if (referral == null)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "請先新增該筆的資料，才允許調整狀態";
                    return apiResult;
                }
                _referralManagementBll.SetReferralStatus(req.SiteId, req.CurrencyId);
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }
            
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            return apiResult;
        }
        
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/ReferralManagement" })]
        public ApiResult<dynamic> SetReferral(SetReferralReq req)
        {
            var apiResult = new ApiResult<dynamic>();
            try
            {
                var referralList = JsonConvert.DeserializeObject<List<ReferralManagement>>(req.Referral);
                _referralManagementBll.SetReferral(referralList);
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            return apiResult;

        }
        
        #endregion
    }
}
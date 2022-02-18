using System;
using System.Collections.Generic;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class DomainController : ControllerBase
    {
        #region Fields
        
        private readonly DomainTableBLL _domainTableBll;
        private readonly AuthorizeBLL _authorizeBll;
        private readonly SelLangBLL _selLangBll;

        #endregion

        #region Constructor

        public DomainController(IServiceProvider provider)
        {
            _domainTableBll = provider.GetRequiredService<DomainTableBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
        }

        #endregion

        #region API Model

        public class DomainListBySiteReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
        }
        
        public class SetDomainReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public string Domain { get; set; }
            public int Status { get; set; }
            public int Sort { get; set; }
        }

        public class DelDomainReq
        {
            public int DId { get; set; }
        }
        
        public class UpdateDomainReq
        {
            public int DId { get; set; }
            public string Domain { get; set; }
            public int Status { get; set; }
            public int Sort { get; set; }
        }
        
        #endregion
        
        /// <summary>
        /// Domain/GetDomainListBySite 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS >  Domain Management 
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public ApiResult<IEnumerable<DomainTable>> GetDomainListBySite(DomainListBySiteReq request)
        {
            var apiResult = new ApiResult<IEnumerable<DomainTable>>
            {
                Data = _domainTableBll.GetDomainListBySite(request.SiteId, request.CurrencyId),
                ErrorCode = (int)ErrorCodeEnum.Success
            };

            return apiResult;
        }
        
        /// <summary>
        /// Domain/SetDomain 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS >  Domain Management 
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public ApiResult SetDomain(SetDomainReq request)
        {
            var apiResult = new ApiResult();
            try
            {
                if (_domainTableBll.SetDomain(request.SiteId, request.CurrencyId, request.Domain, request.Status, request.Sort, _authorizeBll.GetAuthData().UserName) > 0)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                    apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.UpdateFailed;
                    apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062) //匯入資料重複
                {
                    apiResult.ErrorCode = ex.Number;
                    apiResult.Message = "Duplicate data";
                }
                else
                {
                    apiResult.ErrorCode = ex.Number;
                    apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.UpdateFailed;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }

        /// <summary>
        /// Domain/DelDomain 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS >  Domain Management 
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public ApiResult DelDomain(DelDomainReq request)
        {
            var apiResult = new ApiResult();
            if (_domainTableBll.DelDomain(request.DId) > 0)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.UpdateFailed;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }

        /// <summary>
        /// Domain/UpdateDomain 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS >  Domain Management 
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public ApiResult UpdateDomain(UpdateDomainReq request)
        {
            var apiResult = new ApiResult();
            try
            {
                if (_domainTableBll.UpdateDomain(request.DId, request.Domain, request.Status, request.Sort, _authorizeBll.GetAuthData().UserName) > 0)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                    apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.UpdateFailed;
                    apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
                }
            }
            catch (MySqlException ex)
            {

                if (ex.Number == 1062) //匯入資料重複
                {
                    apiResult.ErrorCode = ex.Number;
                    apiResult.Message = "Duplicate data";
                }
                else
                {
                    apiResult.ErrorCode = ex.Number;
                    apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.UpdateFailed;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }
    }
}
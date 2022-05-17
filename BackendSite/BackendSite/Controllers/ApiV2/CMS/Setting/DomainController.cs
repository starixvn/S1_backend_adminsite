using System;
using System.Collections.Generic;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
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

        public class GetListRequest : ApiRequestPageModel
        {
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
        }

        public class CreateRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public string Domain { get; set; }
            public DomainTableStatusEnum Status { get; set; }
            public int Sort { get; set; }
        }

        public class DeleteRequest
        {
            public int Id { get; set; }
        }

        public class UpdateRequest
        {
            public int Id { get; set; }
            public string Domain { get; set; }
            public DomainTableStatusEnum Status { get; set; }
            public int Sort { get; set; }
        }

        #endregion

        /// <summary>
        /// Domain/GetList 取得網域名稱清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS >  Domain Management > Select [Site Name]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/DomainManage" })]
        [HttpPost]
        public ApiListResult<DomainTable> GetList(GetListRequest request)
        {
            var apiResult = new ApiListResult<DomainTable>
            {
                Data = _domainTableBll.GetDomainListBySite(request.SiteIds, request.CurrencyIds, request.PageNumber,
                request.PageSize, out int totalPages, out int rowCount),
                ErrorCode = (int)ErrorCodeEnum.Success,
                PageNumber = request.PageNumber,
                TotalPages = totalPages,
                RowCount = rowCount
            };

            return apiResult;
        }

        /// <summary>
        /// Domain/Create 新增網域名稱
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS >  Domain Management > Select [Site Name] > [Add Domain] > [Save]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/DomainManage" })]
        [HttpPost]
        public ApiResult Create(CreateRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                if (_domainTableBll.SetDomain(request.SiteId, request.CurrencyId,
                    request.Domain, (int)request.Status,
                    request.Sort, _authorizeBll.GetAuthData().UserName) > 0)
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
        /// Domain/Delete 刪除網域名稱
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS >  Domain Management > Select [Site Name] > [Delete]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/DomainManage" })]
        [HttpPost]
        public ApiResult Delete(DeleteRequest request)
        {
            var apiResult = new ApiResult();
            if (_domainTableBll.DelDomain(request.Id) > 0)
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
        /// Domain/Update 更新網域名稱
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS >  Domain Management > Select [Site Name] > [Edit] > [Save]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/DomainManage" })]
        [HttpPost]
        public ApiResult Update(UpdateRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                if (_domainTableBll.UpdateDomain(request.Id, request.Domain,
                    (int)request.Status, request.Sort,
                    _authorizeBll.GetAuthData().UserName) > 0)
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
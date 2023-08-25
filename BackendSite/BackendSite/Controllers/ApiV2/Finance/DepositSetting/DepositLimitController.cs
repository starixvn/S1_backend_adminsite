using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Finance.DepositSetting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class DepositLimitController : ControllerBase
    {
        #region Fields

        private readonly DepositBLL _depositBll;
        private readonly SelLangBLL _selLangBll;

        #endregion

        #region Constructors

        public DepositLimitController(IServiceProvider provider)
        {
            _depositBll = provider.GetRequiredService<DepositBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
        }

        #endregion

        #region API Model

        public class GetListRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
        }

        #endregion

        #region Finance > Deposit Limit Management 

        /// <summary>
        /// DepositLimit/GetList 取得存款限額資料清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Deposit Limit Management > Click[Site Name] [Deposit Type]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public ApiResult<IEnumerable<DepositLimitInfo>> GetList(GetListRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<DepositLimitInfo>>();
            var depositLimitInfos = _depositBll.GetDepositLimitInfos(request.SiteId, request.CurrencyId);
            apiResult.Data = depositLimitInfos;
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;

            return apiResult;
        }

        /// <summary>
        /// DepositLimit/Create 新增存款限額
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Deposit Limit Management > [Submit] > Add
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult Create(ApiRequestWithLog<IEnumerable<DepositLimitModel>> req)
        {
            var apiResult = new ApiResult();

            foreach (var input in req.After)
            {
                var currentUpperPoint = _depositBll.GetDepositLimitInfo(req.SiteId, (int)input.DepositType, req.CurrencyId, (int)input.VipLevel);
                if (currentUpperPoint != null)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.DuplicateData;
                    apiResult.Message = _selLangBll.GetMsg("msg_DuplicateData");
                    return apiResult;
                }
            }

            var result =
                _depositBll.AddDepositLimit(new ParamsWithLog<IEnumerable<DepositLimitModel>>(req, User.Identity.Name));

            if (result)
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
        /// DepositLimit/Update 更新存款限額
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Deposit Limit Management > Click[Site Name] [Deposit Type] > [Edit] > [Save]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult Update(ApiRequestWithLog<IEnumerable<DepositLimitModel>> req)
        {
            var result =
                _depositBll.EditDepositLimit(new ParamsWithLog<IEnumerable<DepositLimitModel>>(req, User.Identity.Name));
            var apiResult = new ApiResult();
            if (result)
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
        /// DepositLimit/Delete 刪除存款限額
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Deposit Limit Management > Clikc[Site Name] [Deposit Type] > [Delete]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiResult Delete(ApiRequestWithLog<DepositLimitModel> req)
        {
            var result =
                _depositBll.DeleteDepositLimitV2(new ParamsWithLog<DepositLimitModel>(req, User.Identity.Name));
            var apiResult = new ApiResult();
            if (result)
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

        #endregion
    }
}
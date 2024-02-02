using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class SlotGamesManagementController : ControllerBase
    {
        #region Fields
        private readonly SlotGamesManagementBLL _slotGamesManagementBLL;
        private readonly GameCasinoManageBLL _gameCasinoManageBLL;
        #endregion

        #region Constructors
        public SlotGamesManagementController(IServiceProvider provider)
        {
            _slotGamesManagementBLL = provider.GetRequiredService<SlotGamesManagementBLL>();
            _gameCasinoManageBLL = provider.GetRequiredService<GameCasinoManageBLL>();
        }
        #endregion

        #region API Model

        public class GetBySiteAndCurrencyRequest
        {
            /// <summary>
            /// 品牌
            /// </summary>
            public IEnumerable<int>? SiteIds { get; set; } = null;
            /// <summary>
            /// 幣別
            /// </summary>
            public int CurrencyId { get; set; } = 0;
        }

        #endregion

        #region Actions
        [HttpPost]
        public ApiResult<IEnumerable<SiteTpListIfo>> GetTpList()
        {
            string requestBody = null;
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                requestBody = reader.ReadToEnd();
            }

            var request = string.IsNullOrWhiteSpace(requestBody) ? new GetBySiteAndCurrencyRequest()
                : JsonSerializer.Deserialize<GetBySiteAndCurrencyRequest>(requestBody);

            var apiResult = new ApiResult<IEnumerable<SiteTpListIfo>>
            {
                Data = _gameCasinoManageBLL.GetTpList(request.SiteIds, request.CurrencyId),
                ErrorCode = 0
            };

            return apiResult;
        }

        /// <summary>
        /// SlotGamesManagement/GetSlotCategoryList
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        ///
        /// Func Path：CMS >  Slot Games Management
        /// </remarks>
        [HttpPost]
        public ApiResult<IEnumerable<SlotCategoryInfo>> GetSlotCategoryList()
        {
            var all = new SlotCategoryInfo { Id = 0, Name = "All (Set Status)" };
            var data = _gameCasinoManageBLL.GetSlotCategoryList();

            var apiResult = new ApiResult<IEnumerable<SlotCategoryInfo>>
            {
                Data = data.Append(all).OrderBy(x => x.Id),
                ErrorCode = 0
            };

            return apiResult;
        }

        /// <summary>
        /// SlotGamesManagement/GetSlotGameList 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS >  Slot Games Management
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 126, 1 })]
        public ApiListResult<SlotGameData> GetSlotGameList(GetSlotGameListReq request)
        {
            try
            {
                var data = _slotGamesManagementBLL.GetSlotGameList(request, out var pageInfo);
                return new ApiListResult<SlotGameData>(ErrorCodeEnum.Success, pageInfo, data);
            }
            catch (Exception ex)
            {
                return new ApiListResult<SlotGameData>((int)ErrorCodeEnum.SystemException, ex.Message);
            }
        }

        /// <summary>
        /// SlotGamesManagement/SetSlotGame 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS >  Slot Games Management 
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 126, 3 })]
        public ApiResult SetSlotGame(SetSlotGameReq request)
        {
            var apiResult = new ApiResult();
            try
            {
                var result = _slotGamesManagementBLL.SetSlotGame(request, User.Identity.Name);
                if (result > 0)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }
            return apiResult;
        }

        /// <summary>
        /// CategoryManagement/GetSlotGameCategoryList 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS >  Category Management 
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 126, 1 })]
        public ApiListResult<SlotGameData> GetSlotGameCategoryList(GetSlotGameCategoryListReq request)
        {
            try
            {
                var data = _slotGamesManagementBLL.GetSlotGameCategoryList(request, out var pageInfo);
                return new ApiListResult<SlotGameData>(ErrorCodeEnum.Success, pageInfo, data);
            }
            catch (Exception ex)
            {
                return new ApiListResult<SlotGameData>((int)ErrorCodeEnum.SystemException, ex.Message);
            }
        }

        /// <summary>
        /// CategoryManagement/SetSlotGameCategory 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS >  Category Management 
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 126, 3 })]
        public ApiResult SetSlotGameCategory(SetSlotGameCategoryReq request)
        {
            var apiResult = new ApiResult();
            try
            {
                var result = _slotGamesManagementBLL.SetSlotGameCategory(request, User.Identity.Name);
                if (result > 0)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }
            return apiResult;
        }

        /// <summary>
        /// SlotGamesManagement/SetSlotGame 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS >  Slot Games Management 
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 126, 3 })]
        public ApiResult SetSlotGameAllCurrency(SetSlotGameReq request)
        {
            var apiResult = new ApiResult();
            try
            {
                var result = _slotGamesManagementBLL.SetSlotGameAllCurrency(request, User.Identity.Name);
                if (result)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }
            return apiResult;
        }

        /// <summary>
        /// SlotGamesManagement/SetNewSlotGame 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS >  Slot Games Management 
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 126, 3 })]
        public ApiResult SetNewSlotGame(SetNewSlotGameReq request)
        {
            var apiResult = new ApiResult();
            try
            {
                var result = _slotGamesManagementBLL.SetNewSlotGame(request, User.Identity.Name);
                if (result)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }
            return apiResult;
        }
        /// <summary>
        /// SlotGamesManagement/GoToPRD
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 126, 3 })]
        public ApiResult SetSlotGameEnvironment(SetSlotGameEnvironment request)
        {
            var apiResult = new ApiResult();
            try
            {
                var result = _slotGamesManagementBLL.SetSlotGameEnvironment(request, User.Identity.Name);
                if (result)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }
            return apiResult;
        }
        #endregion
    }
}

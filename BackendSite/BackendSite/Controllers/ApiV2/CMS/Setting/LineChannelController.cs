using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class LineChannelController : ControllerBase
    {
        private readonly NewAuthorizeBLL _authorizeBll;
        private readonly LineChannelBLL _lineChannelBll;


        #region Constructors

        public LineChannelController(IServiceProvider provider)
        {
            _authorizeBll = provider.GetService<NewAuthorizeBLL>();
            _lineChannelBll = provider.GetService<LineChannelBLL>();
        }

        #endregion

        #region API Model

        public class GetListRequest : ApiRequestPageModel
        {
            public int SiteId { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
        }

        public class GetByIdRequest
        {
            public int Id { get; set; }
        }

        #endregion

        /// <summary>
        /// LineChannel/GetList 取得清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 67, 1 })]
        [HttpPost]
        public ApiListResult<LineChannelModel> GetList(GetListRequest request)
        {
            var datas = _lineChannelBll.GetList(request.SiteId,
                request.CurrencyIds,
                request.PageNumber,
                request.PageSize,
                out int totalPages,
                out int rowCount);

            return new ApiListResult<LineChannelModel>()
            {
                Data = datas,
                ErrorCode = (int)ErrorCodeEnum.Success,
                PageNumber = request.PageNumber,
                TotalPages = totalPages,
                RowCount = rowCount
            };
        }

        /// <summary>
        /// LineChannel/GetById 取得指定資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 67, 1 })]
        [HttpPost]
        public ApiResult<LineChannelModel> GetById(GetByIdRequest request)
        {
            var data = _lineChannelBll.GetById(request.Id);

            return new ApiResult<LineChannelModel>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// LineChannel/Set 新增或更新資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 67, 3 })]
        [HttpPost]
        public ApiResult Set(ApiRequestWithLog<SetLineChannel> request)
        {
            var updator = _authorizeBll.GetAuthData().UserName;
            _lineChannelBll.Set(new ParamsWithLog<SetLineChannel>(request, updator));

            return new ApiResult() { ErrorCode = (int)ErrorCodeEnum.Success };
        }

        /// <summary>
        /// LineChannel/Delete 刪除資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 67, 4 })]
        [HttpPost]
        public ApiResult Delete(ApiRequestWithLog<DeleteLineChannel> request)
        {
            var updator = _authorizeBll.GetAuthData().UserName;
            _lineChannelBll.Delete(new ParamsWithLog<DeleteLineChannel>(request, updator));

            return new ApiResult() { ErrorCode = (int)ErrorCodeEnum.Success };
        }

    }
}

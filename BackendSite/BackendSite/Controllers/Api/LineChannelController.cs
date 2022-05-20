using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LineChannelController : ControllerBase
    {
        private readonly AuthorizeBLL _authorizeBll;
        private readonly LineChannelBLL _lineChannelBll;


        #region Constructors

        public LineChannelController(IServiceProvider provider)
        {
            _authorizeBll = provider.GetService<AuthorizeBLL>();
            _lineChannelBll = provider.GetService<LineChannelBLL>();
        }

        #endregion

        #region API Model

        public class GetListRequest : ApiRequestPageModel
        {
            public int SiteId { get; set; }
        }

        public class GetByIdRequest
        {
            public int Id { get; set; }
        }

        public class SetRequest
        {
            public bool IsAdd { get; set; }
            public int Id { get; set; }
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public string Link { get; set; }
            public int Status { get; set; }
        }

        #endregion

        /// <summary>
        /// LineChannel/GetList 取得清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/LineChannel" })]
        [HttpPost]
        public ApiListResult<LineChannelModel> GetList(GetListRequest request)
        {
            var datas = _lineChannelBll.GetList(request.SiteId,
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/LineChannel" })]
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/LineChannel" })]
        [HttpPost]
        public ApiResult Set(SetRequest request)
        {
            var updator = _authorizeBll.GetAuthData().UserName;

            var setData = new LineChannelDto()
            {
                Id = request.IsAdd ? 0 : request.Id,
                SiteId = request.SiteId,
                CurrencyId = request.CurrencyId,
                Link = request.Link,
                Status = request.Status,
                LastOperator = updator
            };

            _lineChannelBll.Set(setData);

            return new ApiResult() { ErrorCode = (int)ErrorCodeEnum.Success };
        }

        /// <summary>
        /// LineChannel/Delete 刪除資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/LineChannel" })]
        [HttpPost]
        public ApiResult Delete(GetByIdRequest request)
        {
            var updator = _authorizeBll.GetAuthData().UserName;

            _lineChannelBll.Delete(request.Id);

            return new ApiResult() { ErrorCode = (int)ErrorCodeEnum.Success };
        }

    }
}

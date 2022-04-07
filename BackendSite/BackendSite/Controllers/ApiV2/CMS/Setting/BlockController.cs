using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class BlockController : ControllerBase
    {
        #region Fields
        private readonly BlockBLL _blockBLL;
        #endregion

        #region Constructors
        public BlockController(IServiceProvider provider)
        {
            _blockBLL = provider.GetRequiredService<BlockBLL>();
        }
        #endregion

        #region API Model
        public class BlockIPRequest
        {
            public String IP { get; set; }
        }
        public class SetBlockIPRequest: BlockIPRequest
        {
            public string Memo { get; set; }
        }
        #endregion

        #region Action
        /// <summary>
        /// Block/GetBlockIPList 取得被Block的IP List
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResult<BlockIP> GetBlockIPList(BlockIPRequest request)
        {
            var blockIPList = _blockBLL.GetBlockIPList(request.IP);
            return new ApiListResult<BlockIP>
            {
                Data = blockIPList,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        #endregion

        /// <summary>
        /// Block/SetBlockIP 新增Block的IP
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        
        [HttpPost]
        public ApiResult SetBlockIP(SetBlockIPRequest request)
        {
            var result = _blockBLL.SetBlockIP(request.IP, request.Memo, User.Identity.Name);
            if(result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Insert Success."
                };
            }
            else
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Insert Fail."
                };
            }
        }

        /// <summary>
        /// Block/DeleteBlockIP 刪除Block的IP
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>

        [HttpPost]
        public ApiResult DeleteBlockIP(BlockIPRequest request)
        {
            var result = _blockBLL.DeleteBlockIP(request.IP);
            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Delete Success."
                };
            }
            else
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Delete Fail."
                };
            }
        }
    }
}

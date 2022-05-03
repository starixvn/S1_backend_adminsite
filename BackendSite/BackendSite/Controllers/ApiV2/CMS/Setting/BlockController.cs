using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
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
        public class GetBlockIPListRequest : ApiRequestPageModel
        {
            private DateTimeOffset _startDate;
            private DateTimeOffset _endDate;
            public String IP { get; set; }
            public DateTimeOffset StartDate
            {
                get => _startDate;
                set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public DateTimeOffset EndDate
            {
                get => _endDate;
                set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
        }
        public class GetBlockBankAccRequest
        {
            private DateTimeOffset _startDate;
            private DateTimeOffset _endDate;
            public string AccountNum { get; set; }
            public string UserName { get; set; }
            public int Status { get; set; }
            public DateTimeOffset StartDate
            {
                get => _startDate;
                set
                {
                    if (value != null)
                        _startDate = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
                }
            }
            public DateTimeOffset EndDate
            {
                get => _endDate;
                set
                {
                    if (value != null)
                        _endDate = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
                }
            }
        }
        public class BlockBankAccRequest
        {
            public int SysId { get; set; }
            public string AccountNum { get; set; }
        }
        public class BlockAccountSetRequest : BlockBankAccRequest
        {
            public int Status { get; set; }
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
        public ApiListResult<BlockIP> GetBlockIPList(GetBlockIPListRequest request)
        {
            var blockIPList = _blockBLL.GetBlockIPList(request.IP, request.StartDate.DateTime, request.EndDate.DateTime, request.PageSize, request.PageNumber
                ,out int totalPages,out int rowCount);
            return new ApiListResult<BlockIP>
            {
                Data = blockIPList,
                ErrorCode = (int)ErrorCodeEnum.Success,
                TotalPages = totalPages,
                RowCount = rowCount
            };
        }
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
            if (result > 0)
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

        /// <summary>
        /// Block/GetBlockBankAcc 取得BlockBankAccount List
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResult<BlockBankAcc> GetBlockBankAcc(GetBlockBankAccRequest request)
        {
            var blockBankAcc = _blockBLL.GetBlockBankAcc(request.StartDate.DateTime, request.EndDate.DateTime, request.Status,request.AccountNum,request.UserName);
            return new ApiListResult<BlockBankAcc>
            {
                Data = blockBankAcc,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// Block/GetBlockBankAccBlockList 取得blockBankList
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResult<BlockList> GetBlockBankAccBlockList(BlockBankAccRequest request)
        {
            var blockBankList = _blockBLL.GetBlockBankAccBlockList(request.SysId, request.AccountNum);
            return new ApiListResult<BlockList>
            {
                Data = blockBankList,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// Block/GetBlockBankAccBlockList 針對Acoount做lock OR unlock
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult BlockAccountSet(BlockAccountSetRequest request)
        {
            var result = _blockBLL.BlockAccountSet(request.SysId, request.AccountNum, request.Status, User.Identity.Name, request.Memo);
            if (result > 0)
            {
                _blockBLL.SetCustBankInfoLog(request.SysId, request.AccountNum, request.Status);
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success."
                };
            }
            else
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Update Fail."
                };
            }
        }
        #endregion
    }
}

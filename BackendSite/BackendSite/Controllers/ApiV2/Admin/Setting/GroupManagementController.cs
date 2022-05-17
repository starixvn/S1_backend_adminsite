using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackendSite.Controllers.ApiV2.Admin.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class GroupManagementController : ControllerBase
    {
        #region Fields

        private readonly PermissionBLL _permissionBll;
        private readonly AdminGroupBLL _adminGroupBll;

        #endregion

        #region Constructors

        public GroupManagementController(IServiceProvider provider)
        {
            _permissionBll = provider.GetRequiredService<PermissionBLL>();
            _adminGroupBll = provider.GetRequiredService<AdminGroupBLL>();
        }

        #endregion

        #region API Model

        public class GetByIdRequest
        {
            public int GroupId { get; set; }
        }

        public class CreateOrUpdateGroupRequest : GetByIdRequest
        {
            public string GroupName { get; set; }
            public string Remark { get; set; }
            public IEnumerable<FunctionPermission> Permission { get; set; }
        }

        public class FunctionPermission
        {
            public string FuncKey { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string[] SubFuncKeys { get; set; }
        }

        #endregion

        #region Response Model

        public class GetByIdResponse
        {
            public int GroupId { get; set; }
            public string GroupName { get; set; }
            public string Remark { get; set; }

            public IEnumerable<FuncPermissionModel> Permission { get; set; }
        }

        #endregion

        #region Actions

        /// <summary>
        /// GroupManagement/GetList 取得權限群組清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Administration > Group Setting
        /// </remarks>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Admin/GroupAuthoritySetting" })]
        [HttpPost]
        public ApiListResult<AdminGroupModel> GetList(ApiRequestPageModel request)
        {
            var result = new List<AdminGroupModel>();
            var data = _adminGroupBll.GetGroupList(request.PageNumber, request.PageSize, out int totalPages, out int rowCount);

            foreach (var item in data)
            {
                result.Add(new AdminGroupModel()
                {
                    GroupId = (int)item.GroupSEQ,
                    GroupName = item.GroupName,
                    Remark = item.Remark
                });
            }

            var apiResult = new ApiListResult<AdminGroupModel>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success,
                TotalPages = totalPages,
                PageNumber = request.PageNumber,
                RowCount = rowCount
            };

            return apiResult;
        }

        /// <summary>
        /// GroupManagement/Create 新增權限群組
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Administration > Group Setting > [Add Group] > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Admin/GroupAuthoritySetting" })]
        [HttpPost]
        public ApiResult Create(CreateOrUpdateGroupRequest request)
        {
            var apiResult = new ApiResult();
            request.GroupId = 0;
            if (_adminGroupBll.CheckGroupIsExists(request.GroupName, request.GroupId))
            {
                apiResult.ErrorCode = 21;
                apiResult.Message = "Group Name is already taken.";
            }
            else
            {
                var newData = request.Permission.Select(x => new Service.Model.FunctionPermission { FuncKey = x.FuncKey, SubFuncKeys = x.SubFuncKeys });

                var data = new GroupWithPermission()
                {
                    GroupSEQ = request.GroupId,
                    GroupName = request.GroupName,
                    Remark = request.Remark,
                    Permission = newData
                };

                _adminGroupBll.AddGroup(data);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }

            return apiResult;
        }

        /// <summary>
        /// GroupManagement/GetById 取得指定權限群組
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Administration > Group Setting > [Edit]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Admin/GroupAuthoritySetting" })]
        [HttpPost]
        public ApiResult<GetByIdResponse> GetById(GetByIdRequest request)
        {
            var apiResult = new ApiResult<GetByIdResponse>();
            var result = new GetByIdResponse();

            var groupdata = _adminGroupBll.GetGroupByIdOrAll(request.GroupId, false);

            if (groupdata.Count() == 1)
            {
                var group = groupdata.FirstOrDefault();
                result.GroupId = (int)group.GroupSEQ;
                result.GroupName = group.GroupName;
                result.Remark = group.Remark;
            }
            else
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }

            result.Permission = _permissionBll.GetGroupPermissionModel(request.GroupId);

            apiResult.Data = result;
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;

            return apiResult;
        }

        /// <summary>
        /// GroupManagement/Update 更新權限群組
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Administration > Group Setting > [Edit] > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Admin/GroupAuthoritySetting" })]
        [HttpPost]
        public ApiResult Update(CreateOrUpdateGroupRequest request)
        {
            var apiResult = new ApiResult();
            if (request.GroupId == 0)
            {
                apiResult.ErrorCode = 21;
                apiResult.Message = "wrong web-api parameter";
            }
            else
            {

                var newData = request.Permission.Select(x => new Service.Model.FunctionPermission { FuncKey = x.FuncKey, SubFuncKeys = x.SubFuncKeys });

                var data = new GroupWithPermission()
                {
                    GroupSEQ = request.GroupId,
                    GroupName = request.GroupName,
                    Remark = request.Remark,
                    Permission = newData
                };

                _adminGroupBll.UpdateGroup(data);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }

            return apiResult;
        }

        /// <summary>
        /// GroupManagement/Delete 刪除權限群組
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Administration > Group Setting  > [Delete]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Admin/GroupAuthoritySetting" })]
        [HttpPost]
        public ApiResult Delete(GetByIdRequest request)
        {
            var apiResult = new ApiResult();

            if (request.GroupId == 0)
            {
                //這之後可以拔掉，改寫delete是否成功
                apiResult.ErrorCode = 21;
                apiResult.Message = "wrong web-api parameter";
            }
            else
            {
                _adminGroupBll.DeleteGroup((int)request.GroupId);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }

            return apiResult;
        }


        #endregion

    }
}

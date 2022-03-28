using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackendSite.Service.BLL
{
    public class AdminGroupBLL
    {
        private readonly AdminGroupService adminGroupService;
        private readonly ServerInfoService serverInfoService;
        private readonly AuthorizeBLL authorizeBLL;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly PermissionBLL permissionBLL;

        public AdminGroupBLL(ServerInfoService serverInfoService, IHttpContextAccessor httpContextAccessor, AdminGroupService adminGroupService, AuthorizeBLL authorizeBLL, PermissionBLL permissionBLL)
        {
            this.serverInfoService = serverInfoService;
            this.httpContextAccessor = httpContextAccessor;
            this.adminGroupService = adminGroupService;
            this.authorizeBLL = authorizeBLL;
            this.permissionBLL = permissionBLL;
        }

        public IEnumerable<AdminGroup> GetGroupList(int pageNumber,
            int pageSize,
            out int totalPage,
            out int rowCount)
        {
            return adminGroupService.GetGroupList((Math.Abs(pageNumber) - 1) * pageSize,
               Math.Abs(pageSize),
               out totalPage,
               out rowCount);
        }

        public IEnumerable<AdminGroup> GetGroupByIdOrAll(int groupId,bool isAll)
        {
            return adminGroupService.GetGroupByIdOrAll(groupId,isAll);
        }

        public bool CheckGroupIsExists(string GroupName, int? SEQ)
        {
            var all_groups = adminGroupService.GetGroupList(0, 1000, out _, out _);
            return all_groups.Any(x => x.GroupSEQ != SEQ && string.Equals(x.GroupName, GroupName, StringComparison.OrdinalIgnoreCase));
        }

        public void AddGroup(GroupWithPermission GroupInfo)
        {
            adminGroupService.AddGroup(GroupInfo);
        }

        public void DeleteGroup(int GroupSEQ)
        {
            adminGroupService.DeleteGroup(GroupSEQ);
        }

        public AdminGroup GetGroupInfo(int GroupSEQ)
        {
            return adminGroupService.GetGroup(GroupSEQ);
        }

        public void UpdateGroup(GroupWithPermission GroupInfo)
        {
            adminGroupService.UpdateGroup(GroupInfo);
        }
    }
}

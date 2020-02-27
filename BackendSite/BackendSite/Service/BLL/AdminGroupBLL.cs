using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public IEnumerable<AdminGroup> GetGroupList()
        {
            return adminGroupService.GetGroupList();
        }

        public bool CheckGroupIsExists(string GroupName, int? SEQ)
        {
            IEnumerable<AdminGroup> all_groups = adminGroupService.GetGroupList();
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

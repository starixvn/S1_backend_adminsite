using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class PermissionBLL
    {
        private readonly AdminGroupService adminGroupService;
        private readonly PermissionService permissionService;

        public PermissionBLL(PermissionService permissionService, AdminGroupService adminGroupService)
        {
            this.permissionService = permissionService;
            this.adminGroupService = adminGroupService;
        }

        public IEnumerable<FuncPermissionModel> GetPermissionJSONData()
        {
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<FuncPermissionModel>>(permissionService.GetPermissionJSONData());
            return result;
        }

        public IEnumerable<GroupPermisison> GetUserPermission(int GroupSEQ)
        {
            var permission = adminGroupService.GetGroupPermissions(GroupSEQ);
            return permission;
        }

        public IEnumerable<FuncPermissionModel> GetGroupPermissionModel(int GroupSEQ)
        {
            var result = GetPermissionJSONData();
            var permissions = adminGroupService.GetGroupPermissions(GroupSEQ);
            if (permissions.Any())
            {
                foreach (var item in result)
                {
                    item.Settings = SetPermissions(item.Settings, permissions, null);
                }
            }

            return result;
        }

        private List<FuncPermissionSettings> SetPermissions(IEnumerable<FuncPermissionSettings> Settings, IEnumerable<GroupPermisison> GroupPermissions, IEnumerable<UserPermission> UserPermissions)
        {
            List<FuncPermissionSettings> result = new List<FuncPermissionSettings>();
            foreach (var setting in Settings)
            {
                result.Add(SetPermissions(setting, GroupPermissions, UserPermissions));
            }

            return result;
        }

        private FuncPermissionSettings SetPermissions(FuncPermissionSettings Setting, IEnumerable<GroupPermisison> GroupPermissions, IEnumerable<UserPermission> UserPermissions)
        {
            if (Setting.FuncKeys != null && Setting.FuncKeys.Any())
            {
                List<FuncPermissionSettings> functionkeys = new List<FuncPermissionSettings>();
                foreach (var funckeys in Setting.FuncKeys)
                {
                    functionkeys.Add(SetPermissions(funckeys, GroupPermissions, UserPermissions));
                }
                Setting.Enable = functionkeys.Any(x => x.Enable);
                Setting.FuncKeys = functionkeys;
                Setting.IsGroupSetting = functionkeys.Any(x => x.IsGroupSetting);
            }
            else
            {
                var funcByUser = UserPermissions?.SingleOrDefault(i => i.FuncKey == Setting.FuncKey);
                var funcByGroup = GroupPermissions?.SingleOrDefault(x => x.FuncKey == Setting.FuncKey);

                Setting = SetGroupPermission(Setting, funcByGroup);

                if (Setting.InputBoxType == "radio" && funcByGroup != null && funcByUser != null && funcByUser.SubFuncKeys != funcByGroup.SubFuncKeys)
                {

                }
                else
                {
                    Setting = SetUserPermission(Setting, funcByUser);
                }
            }

            return Setting;
        }

        private FuncPermissionSettings SetGroupPermission(FuncPermissionSettings Setting, GroupPermisison GroupPermission)
        {
            if (GroupPermission != null)
            {
                Setting.IsGroupSetting = true;
                Setting.Enable = true;
                if (GroupPermission.SubFuncKeys != null && Setting.InputBoxType != null && Setting.SubFuncKeys.Any())
                {
                    var userSubPermission = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(GroupPermission.SubFuncKeys);
                    if (userSubPermission != null && userSubPermission.Any())
                    {
                        var checkedIndexItemList = new List<int>();
                        foreach (var subFuncKey in userSubPermission)
                        {
                            int idx = Setting.SubFuncKeys.FindIndex(i => i.FuncKey.ToUpper() == subFuncKey.ToUpper());
                            if (idx >= 0) checkedIndexItemList.Add(idx);
                        }

                        Setting.GroupCheckedIndex = checkedIndexItemList.ToArray<int>();
                    }
                }
            }

            return Setting;
        }

        private FuncPermissionSettings SetUserPermission(FuncPermissionSettings Setting, UserPermission UserPermission)
        {
            if (UserPermission != null)
            {
                Setting.Enable = true;
                if (Setting.Enable && UserPermission.SubFuncKeys != null && Setting.InputBoxType != null)
                {
                    var userSubPermission = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(UserPermission.SubFuncKeys);
                    if (userSubPermission != null && userSubPermission.Any())
                    {
                        var checkedIndexItemList = new List<int>();
                        foreach (var subFuncKey in userSubPermission)
                        {
                            int idx = Setting.SubFuncKeys.FindIndex(i => i.FuncKey.ToUpper() == subFuncKey.ToUpper());
                            if (idx >= 0) checkedIndexItemList.Add(idx);
                        }

                        Setting.UserCheckedIndex = checkedIndexItemList.ToArray<int>();
                    }
                }
            }

            return Setting;
        }
    }
}

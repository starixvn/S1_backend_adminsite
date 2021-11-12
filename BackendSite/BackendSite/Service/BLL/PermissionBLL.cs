using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackendSite.Service.BLL
{
    public class PermissionBLL
    {
        private readonly AdminGroupService adminGroupService;
        private readonly PermissionService permissionService;
        private readonly AuthorizeBLL authorizeBLL;
        private readonly AdminUserService adminUserService;

        public PermissionBLL(PermissionService permissionService, AdminGroupService adminGroupService, AuthorizeBLL authorizeBLL, AdminUserService adminUserService)
        {
            this.permissionService = permissionService;
            this.adminGroupService = adminGroupService;
            this.authorizeBLL = authorizeBLL;
            this.adminUserService = adminUserService;
        }

        public IEnumerable<FuncPermissionModel> GetPermissionJSONData()
        {
            var result = JsonConvert.DeserializeObject<IEnumerable<FuncPermissionModel>>(permissionService.GetPermissionJSONData());
            return result;
        }

        public IEnumerable<GroupPermisison> GetAllPermission()
        {
            var result = new List<GroupPermisison>();
            
            var permissionDatas = GetPermissionJSONData();
            foreach (var func in permissionDatas)
            {
                foreach (var funcKeyItem in func.Settings.Where(funcKeyItem => funcKeyItem != null))
                {
                    if (funcKeyItem.SubFuncKeys != null)
                    {
                        var subFuncKeyDatas = funcKeyItem.SubFuncKeys.Select(x => x.FuncKey);
                        result.AddRange(subFuncKeyDatas.Select(
                            subFuncKeyItem => new GroupPermisison {GroupSEQ = 0, FuncKey = funcKeyItem.FuncKey, SubFuncKeys = subFuncKeyItem}));
                    }
                    else
                    {
                        result.Add(new GroupPermisison
                        {
                            GroupSEQ = 0,
                            FuncKey = funcKeyItem.FuncKey,
                            SubFuncKeys = null
                        });
                    }
                }
            }
            return result;
        }

        public IEnumerable<GroupPermisison> GetUserPermission(int GroupSEQ)
        {
            var permission = adminGroupService.GetGroupPermissions(GroupSEQ);
            return permission;
        }

        public GroupPermisison GetUserPermission(int GroupSEQ, string FuncKey)
        {
            var permission = adminGroupService.GetGroupPermissions(GroupSEQ)?.FirstOrDefault(x => string.Equals(x.FuncKey, FuncKey, StringComparison.OrdinalIgnoreCase));
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
            var result = new List<FuncPermissionSettings>();
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
                var functionkeys = new List<FuncPermissionSettings>();
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
                            var idx = Setting.SubFuncKeys.FindIndex(i => i.FuncKey.ToUpper() == subFuncKey.ToUpper());
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
                            var idx = Setting.SubFuncKeys.FindIndex(i => i.FuncKey.ToUpper() == subFuncKey.ToUpper());
                            if (idx >= 0) checkedIndexItemList.Add(idx);
                        }

                        Setting.UserCheckedIndex = checkedIndexItemList.ToArray<int>();
                    }
                }
            }

            return Setting;
        }

        public bool CheckPermission(string FuncKey, string SubFuncKey)
        {
            var authData = authorizeBLL.GetAuthData();
            var loginUser = adminUserService.GetUser(authData.UserId);

            var result = false;
            if (loginUser.IsSuper)
            {
                result = true;
            }
            else
            {
                if (loginUser.GroupSEQ == null)
                {
                    result = false;
                }
                else
                {
                    var permissions = GetUserPermission((int)loginUser.GroupSEQ);
                    if (permissions != null && permissions.Any())
                    {
                        var funcItem = permissions.FirstOrDefault(x => string.Equals(x.FuncKey, FuncKey, StringComparison.OrdinalIgnoreCase));
                        var subItem = funcItem.SubFuncKeys == null ? new string[] { } : Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(funcItem.SubFuncKeys);

                        if (!string.IsNullOrEmpty(SubFuncKey))
                        {
                            if (subItem.Any())
                            {
                                result = subItem.Any(s => string.Equals(s, SubFuncKey, StringComparison.OrdinalIgnoreCase));
                            }
                        }
                        else
                        {
                            result = funcItem != null;
                        }
                    }
                }
            }

            return result;
        }

        public bool CheckPermission(int GroupSEQ, string FuncKey, string SubFuncKey)
        {
            var authData = authorizeBLL.GetAuthData();
            var loginUser = adminUserService.GetUser(authData.UserId);

            var result = false;
            var permissions = GetUserPermission(GroupSEQ);
            if (permissions != null && permissions.Any())
            {
                var funcItem = permissions.FirstOrDefault(x => string.Equals(x.FuncKey, FuncKey, StringComparison.OrdinalIgnoreCase));
                var subItem = funcItem.SubFuncKeys == null ? new string[] { } : Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(funcItem.SubFuncKeys);

                if (!string.IsNullOrEmpty(SubFuncKey))
                {
                    if (subItem.Any())
                    {
                        result = subItem.Any(s => string.Equals(s, SubFuncKey, StringComparison.OrdinalIgnoreCase));
                    }
                }
                else
                {
                    result = funcItem != null;
                }
            }

            return result;
        }

        public IEnumerable<string> GetSubFunckeys(string FuncKey)
        {
            var authData = authorizeBLL.GetAuthData();
            var loginUser = adminUserService.GetUser(authData.UserId);
            IEnumerable<string> result = null;
            if (loginUser.IsSuper)
            {
                var initDatas = GetPermissionJSONData();
                foreach (var func in initDatas)
                {
                    var item = GetFuncItem(func.Settings, FuncKey);
                    if (item != null)
                    {
                        result = item.SubFuncKeys.Select(x => x.FuncKey);
                        break;
                    }
                }
            }
            else if (loginUser.GroupSEQ != null)
            {
                var permissions = GetUserPermission((int)loginUser.GroupSEQ, FuncKey);
                if (permissions != null && permissions.SubFuncKeys != null)
                {
                    result = JsonConvert.DeserializeObject<IEnumerable<string>>(permissions.SubFuncKeys);
                }
            }

            return result;
        }

        private FuncPermissionSettings GetFuncItem(List<FuncPermissionSettings> Settings, string FuncKey)
        {
            FuncPermissionSettings result = null;
            foreach (var func in Settings)
            {
                if (string.Equals(func.FuncKey, FuncKey))
                {
                    result = func;
                    break;
                }
                else if (func.FuncKeys != null && func.FuncKey.Any())
                {
                    return GetFuncItem(func.FuncKeys, FuncKey);
                }
            }

            return result;
        }

        public bool DashBoardAuthorize()
        {
            var result = false;

            var authData = authorizeBLL.GetAuthData();
            var loginUser = adminUserService.GetUser(authData.UserId);

            if (loginUser.IsSuper)
            {
                result = true;
            }
            else if (loginUser.GroupSEQ != null)
            {
                var permissions = GetUserPermission((int)loginUser.GroupSEQ, "Home/DashboardNew");
                if (permissions != null)
                {
                    result = true;
                }
            }

            return result;
        }
    }
}

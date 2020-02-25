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
        private readonly PermissionService permissionService;

        public PermissionBLL(PermissionService permissionService)
        {
            this.permissionService = permissionService;
        }

        public IEnumerable<FuncPermissionModel> GetPermissionJSONData()
        {
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<FuncPermissionModel>>(permissionService.GetPermissionJSONData());
            return result;
        }

        public List<FuncPermissionSettings> SetPermissions(IEnumerable<FuncPermissionSettings> Settings, IEnumerable<GroupPermisison> GroupPermissions, IEnumerable<UserPermission> UserPermissions)
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
            if (Setting.FunctionKeys != null && Setting.FunctionKeys.Any())
            {
                List<FuncPermissionSettings> functionkeys = new List<FuncPermissionSettings>();
                foreach (var funckeys in Setting.FunctionKeys)
                {
                    functionkeys.Add(SetPermissions(funckeys, GroupPermissions, UserPermissions));
                }
                Setting.Enable = functionkeys.Any(x => x.Enable);
                Setting.FunctionKeys = functionkeys;
                Setting.IsGroupSetting = functionkeys.Any(x => x.IsGroupSetting);
            }
            else
            {
                var funcByUser = UserPermissions?.SingleOrDefault(i => i.FuncKey == Setting.FunctionKey);
                var funcByGroup = GroupPermissions?.SingleOrDefault(x => x.FuncKey == Setting.FunctionKey);

                Setting = SetGroupPermission(Setting, funcByGroup);

                if (Setting.InputBoxType == "radio" && funcByGroup != null && funcByUser != null && funcByUser.SubFuncKey != funcByGroup.SubFuncKey)
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
                if (GroupPermission.SubFuncKey != null && Setting.InputBoxType != null && Setting.SubFunctionKeys.Any())
                {
                    var userSubPermission = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(GroupPermission.SubFuncKey);
                    if (userSubPermission != null && userSubPermission.Any())
                    {
                        var checkedIndexItemList = new List<int>();
                        foreach (var subFuncKey in userSubPermission)
                        {
                            int idx = Setting.SubFunctionKeys.FindIndex(i => i.FunctionKey.ToUpper() == subFuncKey.ToUpper());
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
                if (Setting.Enable && UserPermission.SubFuncKey != null && Setting.InputBoxType != null)
                {
                    var userSubPermission = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(UserPermission.SubFuncKey);
                    if (userSubPermission != null && userSubPermission.Any())
                    {
                        var checkedIndexItemList = new List<int>();
                        foreach (var subFuncKey in userSubPermission)
                        {
                            int idx = Setting.SubFunctionKeys.FindIndex(i => i.FunctionKey.ToUpper() == subFuncKey.ToUpper());
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

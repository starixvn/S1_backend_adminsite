using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.BLL
{
    public class MenuBLL
    {
        private readonly AdminGroupService adminGroupService;
        private readonly MenuService menuService;
        private readonly AdminUserService adminUserService;
        private readonly AuthorizeBLL authorizeBLL;

        public MenuBLL(AdminGroupService adminGroupService, MenuService menuService, AuthorizeBLL authorizeBLL,
            AdminUserService adminUserService)
        {
            this.adminGroupService = adminGroupService;
            this.menuService = menuService;
            this.authorizeBLL = authorizeBLL;
            this.adminUserService = adminUserService;
        }

        public List<MenuItem> GetMenuData(bool allFlag, out ErrorCodeEnum errorCodeEnum)
        {
            //default success
            errorCodeEnum = ErrorCodeEnum.Success;

            var menuData = JsonConvert.DeserializeObject<List<MenuItem>>(menuService.GetMenuData());

            var authData = authorizeBLL.GetAuthData();

            if (authData == null)
            {
                //get auth fail
                errorCodeEnum = ErrorCodeEnum.Fail;
                return null;
            }

            var loginUser = adminUserService.GetUser(authData.UserId);

            if (loginUser == null)
            {
                //get user fail
                errorCodeEnum = ErrorCodeEnum.Fail;
                return null;
            }

            if (allFlag)
            {
                return menuData;
            }

            if (loginUser.IsSuper)
            {
                return menuData;
            }

            if (loginUser.GroupSEQ == null)
            {
                //get user's group fail
                errorCodeEnum = ErrorCodeEnum.Fail;
                return null;
            }

            var permissions = adminGroupService.GetGroupPermissions((int) loginUser.GroupSEQ);

            return CheckMenuPermission(menuData, permissions);
        }

        private List<MenuItem> CheckMenuPermission(IEnumerable<MenuItem> Menu,
            IEnumerable<GroupPermisison> groupPermisisons)
        {
            var result = new List<MenuItem>();
            foreach (var item in Menu)
            {
                if (item.NoAuth)
                {
                    result.Add(item);
                }
                else
                {
                    if (item.Childs != null && item.Childs.Any())
                    {
                        var childs = CheckMenuPermission(item.Childs, groupPermisisons);
                        if (childs != null && childs.Any())
                        {
                            item.Childs = childs;
                            result.Add(item);
                        }
                    }

                    if (item.Childs == null && !string.IsNullOrEmpty(item.FuncKey) && groupPermisisons.Any(x =>
                        string.Equals(x.FuncKey, item.FuncKey, StringComparison.OrdinalIgnoreCase)))
                    {
                        result.Add(item);
                    }
                }
            }

            return result;
        }
    }
}
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class MenuBLL
    {
        private readonly AdminGroupService adminGroupService;
        private readonly MenuService menuService;
        private readonly AdminUserService adminUserService;
        private readonly AuthorizeBLL authorizeBLL;

        public MenuBLL(AdminGroupService adminGroupService, MenuService menuService, AuthorizeBLL authorizeBLL, AdminUserService adminUserService)
        {
            this.adminGroupService = adminGroupService;
            this.menuService = menuService;
            this.authorizeBLL = authorizeBLL;
            this.adminUserService = adminUserService;
        }

        public List<MenuItem> GetMenuData()
        {
            var result = new List<MenuItem>();
            var menuDatas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MenuItem>>(menuService.GetMenuData());
            var authData = authorizeBLL.GetAuthData();
            if (authData != null)
            {
                var loginUser = adminUserService.GetUser(authData.UserId);
                if (loginUser.IsSuper)
                {
                    result = menuDatas;
                }
                else if (loginUser != null && loginUser.GroupSEQ != null)
                {
                    var permissions = adminGroupService.GetGroupPermissions((int)loginUser.GroupSEQ);
                    return CheckMenuPermission(menuDatas, permissions);
                }
            }

            return result;
        }

        private List<MenuItem> CheckMenuPermission(IEnumerable<MenuItem> Menu, IEnumerable<GroupPermisison> groupPermisisons)
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

                    if (item.Childs == null && !string.IsNullOrEmpty(item.FuncKey) && groupPermisisons.Any(x => string.Equals(x.FuncKey, item.FuncKey, StringComparison.OrdinalIgnoreCase)))
                    {
                        result.Add(item);
                    }
                }
            }

            return result;
        }
    }
}

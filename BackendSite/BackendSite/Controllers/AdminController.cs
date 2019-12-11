using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AdminBLL adminBLL;
        private readonly SelLangBLL selLangBLL;

        public AdminController(AdminBLL adminBLL, SelLangBLL selLangBLL)
        {
            this.adminBLL = adminBLL;
            this.selLangBLL = selLangBLL;
        }

        public IActionResult AddAccount()
        {
            return View();
        }

        public IActionResult AccountList()
        {
            IEnumerable<AdminUser> adminUsers = adminBLL.AccountList();
            return View(adminUsers.ToList());
        }

        public IActionResult AccountEdit(int userId)
        {
            AdminUser adminUser = adminBLL.GetAccount(userId);
            return View(adminUser);
        }
        
    }
}
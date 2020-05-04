using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers
{
    public class MessageController : Controller
    {
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Message/Announcement" })]
        public IActionResult Announcement()
        {
            return View();
        }

        public IActionResult PersonalMsgBatch()
        {
            return View();
        }
    }
}
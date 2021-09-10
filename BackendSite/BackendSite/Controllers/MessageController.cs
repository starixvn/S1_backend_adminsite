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
        public IActionResult ChangeVipLevelBatch()
        {
            return View();
        }
        public IActionResult WithdrawalKYCControl()
        {
            return View();
        }
    }
}
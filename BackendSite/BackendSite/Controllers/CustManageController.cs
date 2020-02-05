using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers
{
    public class CustManageController : Controller
    {
        private readonly CustInfoBLL custInfoBLL;
        private readonly ExcelBLL excelBLL;

        public CustManageController(CustInfoBLL custInfoBLL, ExcelBLL excelBLL)
        {
            this.custInfoBLL = custInfoBLL;
            this.excelBLL = excelBLL;
        }

        [HttpGet]
        public IActionResult CustInfoList()
        {
            return View();
        }
        [HttpGet]
        public IActionResult CustInfo(int CustId, string TransId)
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetCustPhotoId(int CustId)
        {
            var custPhotoId = custInfoBLL.GetCustPhotoId(CustId);
            if (custPhotoId == null)
            {
                return StatusCode(404);
            }
            var image = custPhotoId.PhotoIdPic;
            return File(image, "image/jpeg");
        }

        public class CustInfoReq
        {
            public string UserName { get; set; }
            public string TransId { get; set; }
            public string TpUserName { get; set; }
        }

        [HttpPost]
        public IActionResult ExportCustInfoList(CustInfoReq custInfoReq)
        {
            var ExportCustInfo = custInfoBLL.GetCustInfoExportList(custInfoReq.UserName, custInfoReq.TransId, custInfoReq.TpUserName);

            var stream = excelBLL.Export(ExportCustInfo);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{nameof(ExportCustInfo)}{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }
    }
}
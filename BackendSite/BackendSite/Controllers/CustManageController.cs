using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Attributes;
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
            public string SelectSite { get; set; }
            public long? StartDate { get; set; }
            public long? EndDate { get; set; }
            public int? BirDate { get; set; }
            public int? BirMonth { get; set; }
            public int? BirYear { get; set; }
            public string UserName { get; set; }
            public string AccountName { get; set; }
            public string LoginIp { get; set; }
            public string TransId { get; set; }
            public string TpUserName { get; set; }
            public string Email { get; set; }
            public string PhoneNo { get; set; }
            public int? AffId { get; set; }
            public string SelectCurrency { get; set; }
            public string BankAccountNumber { get; set; }
            public string SelectStatus { get; set; }
        }

        [HttpPost]
        [RequestSizeLimit(40000000)]
        public IActionResult ExportCustInfoList(CustInfoReq custInfoReq)
        {
            List<int> selectSite = new List<int>();
            List<int> selectCurrency = new List<int>();
            List<int> selectStatus = new List<int>();
            if (custInfoReq.SelectSite != null)
            {
                selectSite = Array.ConvertAll<string, int>(custInfoReq.SelectSite.Split(','), s => int.Parse(s)).ToList();
            }
            if (custInfoReq.SelectCurrency != null)
            {
                selectCurrency = Array.ConvertAll<string, int>(custInfoReq.SelectCurrency.Split(','), s => int.Parse(s)).ToList();
            }
            if (custInfoReq.SelectStatus != null)
            {
                selectStatus = Array.ConvertAll<string, int>(custInfoReq.SelectStatus.Split(','), s => int.Parse(s)).ToList();
            }        
            var ExportCustInfo = custInfoBLL.GetCustInfoExportList(selectSite, custInfoReq.BirDate, custInfoReq.BirMonth, custInfoReq.BirYear, custInfoReq.UserName, custInfoReq.AccountName, custInfoReq.LoginIp, custInfoReq.TransId, custInfoReq.TpUserName, custInfoReq.Email, custInfoReq.PhoneNo, custInfoReq.StartDate, custInfoReq.EndDate, custInfoReq.AffId, selectCurrency, custInfoReq.BankAccountNumber, selectStatus);

            var stream = excelBLL.Export(ExportCustInfo);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{nameof(ExportCustInfo)}{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TeleCSD" })]
        public IActionResult TeleCSD()
        {
            return View();
        }

    }
}
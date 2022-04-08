using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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

        #region Old Code

        // 這隻For現行後台使用，有另一支API For Frontend Team，
        // 如有調整邏輯，請兩邊同步調整+測試。(api/CustManage/GetCustPhotoId)
        [ApiExplorerSettings(IgnoreApi = true)]
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
            public long? TagStartDate { get; set; }
            public long? TagEndDate { get; set; }
            public long? StatusStartDate { get; set; }
            public long? StatusEndDate { get; set; }
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
            public string AffCode { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public string SelectCurrency { get; set; }
            public string BankAccountNumber { get; set; }
            public string SelectStatus { get; set; }
            public string SelectTag { get; set; }
            public bool ExportTransferWallet { get; set; } = false;
        }

        // 這隻For現行後台使用，有另一支API For Frontend Team，
        // 如有調整邏輯，請兩邊同步調整+測試。(api/CustManage/ExportCustInfoList)
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        [RequestSizeLimit(40000000)]
        public async Task<IActionResult> ExportCustInfoListAsync(CustInfoReq req)
        {
            var selectSite = new List<int>();
            var selectCurrency = new List<int>();
            var selectStatus = new List<int>();
            var selectTag = new List<int>();
            if (req.SelectSite != null)
            {
                selectSite = Array.ConvertAll(req.SelectSite.Split(','), int.Parse).ToList();
            }
            if (req.SelectCurrency != null)
            {
                selectCurrency = Array.ConvertAll(req.SelectCurrency.Split(','), int.Parse).ToList();
            }
            if (req.SelectStatus != null)
            {
                selectStatus = Array.ConvertAll(req.SelectStatus.Split(','), int.Parse).ToList();
            }
            if (req.SelectTag != null)
            {
                selectTag = Array.ConvertAll(req.SelectTag.Split(','), int.Parse).ToList();
            }
            
            var startDate = req.StartDate != null ? DateTimeExtensions.UnixToDateTime((long) req.StartDate) : (DateTime?) null;
            var endDate = req.EndDate != null ? DateTimeExtensions.UnixToDateTime((long) req.EndDate) : (DateTime?) null;
            var tagStartDate = req.TagStartDate != null ? DateTimeExtensions.UnixToDateTime((long) req.TagStartDate) : (DateTime?) null;
            var tagEndDate = req.TagEndDate != null ? DateTimeExtensions.UnixToDateTime((long) req.TagEndDate) : (DateTime?) null;
            var statusStartDate = req.StatusStartDate != null ? DateTimeExtensions.UnixToDateTime((long) req.StatusStartDate) : (DateTime?) null;
            var statusEndDate = req.StatusEndDate != null ? DateTimeExtensions.UnixToDateTime((long) req.StatusEndDate) : (DateTime?) null;

            
            if (req.ExportTransferWallet)
            {
                var data = GetCustInfoExportList<CustInfoDetailForExport>(req, selectSite, startDate, endDate, tagStartDate, tagEndDate, statusStartDate, statusEndDate, selectCurrency, selectStatus, selectTag);
                data.ToList().ForEach(s => s.RegTime = new DateTimeOffset(s.RegTime.DateTime.AddHours(8), TimeSpan.FromHours(8)));
                var exportDatas = new List<CustInfoDetailForExport>();
                var tasks = data.Select(async x =>
                {
                    var exprotdata = await custInfoBLL.GetTransferWalletAsync(x);
                    exportDatas.Add(exprotdata);
                });
                await Task.WhenAll(tasks);
                //如果該名user的SabaWallet,PtWallet,SeamlessWallet,BonusWallet 皆為0就不export該名user
                exportDatas.RemoveAll(x => x.SabaWallet == 0 && x.PTWallet == 0 && x.SeamlessWallet == 0 && x.BonusWallet == 0);
                var stream = excelBLL.ExportV2(exportDatas);
                return excelBLL.ExcelFile(stream, "CustInfoList");
            }
            else
            {
                var result = GetCustInfoExportList<CustInfoDetailForExportV2>(req, selectSite, startDate, endDate, tagStartDate, tagEndDate, statusStartDate, statusEndDate, selectCurrency, selectStatus, selectTag);
                result.ToList().ForEach(s => s.RegTime = new DateTimeOffset(s.RegTime.DateTime.AddHours(8), TimeSpan.FromHours(8)));
                var stream = excelBLL.ExportV2(result);
                return excelBLL.ExcelFile(stream, "CustInfoList");
            }
        }

        private IEnumerable<T> GetCustInfoExportList<T>(CustInfoReq req, IEnumerable<int> selectSite, DateTime? startDate, DateTime? endDate,
            DateTime? tagStartDate, DateTime? tagEndDate, DateTime? statusStartDate, DateTime? statusEndDate,
            IEnumerable<int> selectCurrency, IEnumerable<int> selectStatus, IEnumerable<int> selectTag)
        {
            var data = custInfoBLL.GetCustInfoExportList<T>(selectSite,
                req.BirDate,
                req.BirMonth,
                req.BirYear,
                req.UserName,
                req.AccountName,
                req.LoginIp,
                req.TransId,
                req.TpUserName,
                req.Email,
                req.PhoneNo,
                startDate,
                endDate,
                tagStartDate,
                tagEndDate,
                statusStartDate,
                statusEndDate,
                req.AffCode,
                selectCurrency,
                req.BankAccountNumber,
                selectStatus,
                selectTag);
            return data;
        }

        #endregion

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TeleCSD" })]
        public IActionResult TeleCSD()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TagManagement" })]
        public IActionResult TagManagement()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/RegistrationManagement" })]
        public IActionResult RegistrationManagement()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/DisableUserForGame" })]
        public IActionResult DisableUserForGame()
        {
            return View();
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/UserBetLimit" })]
        public IActionResult UserBetLimit()
        {
            return View();
        }



        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TpFailRecordReport" })]
        public IActionResult TpFailRecordReport()
        {
            return View();
        }
    }
}

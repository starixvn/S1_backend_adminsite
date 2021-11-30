using System;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using LazZiya.ImageResize;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using BackendSite.Service.Library;
using System.Transactions;

namespace BackendSite.Service.BLL
{
    public class DepositBLL
    {
        private readonly DepositService depositService;
        private readonly ServerInfoService serverInfoService;
        private readonly AdjustmentService adjustmentService;
        private readonly CustInfoBLL custInfoBLL;
        private readonly AuthorizeBLL authorizeBLL;
        private readonly Common common;
        private readonly CustInfoService custInfoService;

        public DepositBLL(DepositService depositService, ServerInfoService serverInfoService, AdjustmentService adjustmentService, CustInfoBLL custInfoBLL, AuthorizeBLL authorizeBLL, Common common, CustInfoService custInfoService)
        {
            this.depositService = depositService;
            this.serverInfoService = serverInfoService;
            this.adjustmentService = adjustmentService;
            this.custInfoBLL = custInfoBLL;
            this.authorizeBLL = authorizeBLL;
            this.common = common;
            this.custInfoService = custInfoService;
        }

        public IEnumerable<DepositRequest> GetDepositRequestList(IEnumerable<int> selectSite, IEnumerable<int> depositType, DateTime startDate, DateTime endDate, string depositId, string userName, IEnumerable<int> currencyId, IEnumerable<int> status, IEnumerable<int> levelStatus,int pageSize,int amountType,string amount ,ref int pageNumber, out int totalPages, out int rowCount, ref List<string> currencyAmount)
        {
            var serverInfo = serverInfoService.GetModel();/*Check SiteID By Permission*/
            return depositService.GetDepositRequestList(selectSite, depositType, startDate, endDate, serverInfo.SiteId/*Not require*/, depositId, userName, currencyId, status, levelStatus,pageSize, amountType, amount,ref pageNumber, out totalPages, out rowCount,ref currencyAmount);
        }

        public IEnumerable<DepositRequest> GetDepositListByCust(int CustId,
            string TransId,
            int? Status,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            int pageSize,
            int pageNumber,
            out int totalPages,
            out int rowCount,
            out decimal totalAmount)
        {
            return depositService.GetDepositListByCust(CustId,
                TransId,
                Status,
                startDate,
                endDate,
                Math.Abs(pageSize),
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                out totalPages,
                out rowCount,
                out totalAmount);
        }

        public int CreateDeposit(DepositDetailInfo deposit, IFormFile receipt)
        {
            var depositInfo = new DepositInfo3_1
            {
                AccountName = deposit.AccountHolderName,
                CurrencyId = deposit.CurrencyId,
                AccountNo = deposit.AccountNumber,
                SysId = deposit.SysId,
                Ver = 1
            };
            deposit.DepositInfo = JsonConvert.SerializeObject(depositInfo);

            var custId = (string.IsNullOrEmpty(deposit.SabaId)) ? null : common.TpIdToCustId(deposit.SabaId);
            var customer = custInfoBLL.GetCustInfoDetails(custId, deposit.UserName, deposit.SiteId);
            if (customer == null)
            {
                return -1;
            }
            deposit.CurrencyId = customer.CurrencyId;
            deposit.CustId = customer.CustId;
            deposit.UserName = customer.UserName;

            var creator = authorizeBLL.GetAuthData();
            deposit.Operator = creator.UserName;


            var transId = depositService.InsertDepositRequest(deposit);

            if (!string.IsNullOrEmpty(transId) && receipt != null)
            {
                InsertDepositReceipt(receipt, deposit.CustId, transId);
            }

            if (!string.IsNullOrEmpty(transId))
            {
                return 1;
            }
            return 0;
        }

        private int InsertDepositReceipt(IFormFile depositReceipt, int custId, string transId)
        {
            var processedReceipt = ProcessImage(depositReceipt);

            var depositId = ValidateDepositReceiptPic(transId);

            return depositService.SetDepositReceiptPic(custId, depositId, processedReceipt.FileName, processedReceipt.FileBytes); ;
        }

        private ProcessedDepositReceipt ProcessImage(IFormFile depositReceipt)
        {
            var processedDepositReceipt = new ProcessedDepositReceipt();
            if (_contentTypesCheck.ContainsKey(depositReceipt.ContentType))
            {
                var xSize = 500m;
                var apiResult = new ApiResult();
                decimal xCell = 1;

                using var fileStream = depositReceipt.OpenReadStream();
                using var uploadedImage = Image.FromStream(fileStream);
                using var ms = new System.IO.MemoryStream();

                if (uploadedImage.Width > xSize || uploadedImage.Height > xSize)
                {
                    if (uploadedImage.Width > uploadedImage.Height)
                    {
                        xCell = xSize / uploadedImage.Width;
                    }
                    else
                    {
                        xCell = xSize / uploadedImage.Height;
                    }
                }

                var graphicOptions = new GraphicOptions
                {
                    CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed,
                    SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed
                };
                var img = ImageResize.Scale(uploadedImage, Convert.ToInt32(uploadedImage.Width * xCell), Convert.ToInt32(uploadedImage.Height * xCell), graphicOptions);
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                var fileName = depositReceipt.FileName;
                fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
                fileName = $"{fileName.Substring(0, fileName.LastIndexOf('.'))}.jpg";

                var fileBytes = ms.ToArray();

                processedDepositReceipt.FileName = fileName;
                processedDepositReceipt.FileBytes = fileBytes;
            }

            return processedDepositReceipt;
        }

        private int ValidateDepositReceiptPic(string transId)
        {
            var depositId = 0;
            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out depositId);
            return depositId;
        }

        public class ProcessedDepositReceipt
        {
            public string FileName { get; set; }
            public byte[] FileBytes { get; set; }
        }

        public DepositRequest GetDepositRequest(string transId)
        {
            var serverInfo = serverInfoService.GetModel();

            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out var depositId);
            return depositService.GetDepositRequest(depositId, serverInfo.SiteId/*Not require*/);/*Check SiteID By Permission*/
        }

        public IEnumerable<Comments> GetDepositRequestComment(string transId)
        {
            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out var depositId);
            var depositRequest = GetDepositRequest(transId);
            return depositService.GetDepositRequestComment(depositId, depositRequest.SiteId);
        }

        public int DepositApproved(string transId, decimal actualDeposit, string comment, string Operator)
        {
            var forStatus = new List<int> {(int)DepositStatus.Pending};
            var depositRequest = GetDepositRequest(transId);

            return depositService.SetDepositInfo(transId, depositRequest.SiteId, depositRequest.Amount, comment, Operator, forStatus, DepositStatus.Approved, depositRequest);
        }

        public int DepositRejectd(string transId, decimal actualDeposit, string comment, string Operator)
        {
            var forStatus = new List<int> {(int)DepositStatus.Pending};

            var depositRequest = GetDepositRequest(transId);
            return depositService.SetDepositInfo(transId, depositRequest.SiteId, actualDeposit, comment, Operator, forStatus, DepositStatus.Rejected, null);
        }

        public int DepositDraft(string transId, decimal actualDeposit, string comment, string Operator)
        {
            var forStatus = new List<int>
            {
                (int) DepositStatus.Pending,
                (int) DepositStatus.OnHold
            };

            var depositRequest = GetDepositRequest(transId);
            return depositService.SetDepositInfo(transId, depositRequest.SiteId, actualDeposit, comment, Operator, forStatus, DepositStatus.Draft, null);
        }
        public int DepositOnHold(string transId, decimal actualDeposit, string comment, string Operator)
        {
            var forStatus = new List<int>();
            forStatus.Add((int)DepositStatus.Pending);
            var depositRequest = GetDepositRequest(transId);
            return depositService.SetDepositInfo(transId, depositRequest.SiteId, actualDeposit, comment, Operator, forStatus, DepositStatus.OnHold, null);
        }
        public int DepositPending(string transId, decimal actualDeposit, string comment, string Operator)
        {
            var forStatus = new List<int>();
            forStatus.Add((int)DepositStatus.OnHold);
            var depositRequest = GetDepositRequest(transId);
            return depositService.SetDepositInfo(transId, depositRequest.SiteId, actualDeposit, comment, Operator, forStatus, DepositStatus.Pending, null);
        }
        public DepositReceipt GetDepositReceipt(string transId)
        {
            var depositId = 0;
            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out depositId);
            var depositRequest = GetDepositRequest(transId);
            return depositService.GetDepositReceipt(depositId, depositRequest.SiteId);
        }

        public IEnumerable<DepositInfoForExport> GetDepositExportList(string selectSite, string depositType, DateTime startDate, DateTime endDate, string depositId, string userName, string currencyId, string status,string levelStatus,int amountType,string amount)
        {
            return depositService.GetDepositExportList(selectSite, depositType, startDate, endDate, depositId, userName, currencyId, status, levelStatus, amountType, amount);
        }
        public IEnumerable<DepositLimitCurrencyInfo> GetDepositLimitCurrencyList(int siteId, int depositType) => depositService.GetDepositLimitCurrencyList(siteId, depositType);

        public IEnumerable<DepositLimitInfo> GetDepositLimitInfos(int siteId, int depositType, int currencyId) => depositService.GetDepositLimitInfos(siteId, depositType, currencyId);

        public bool AddDepositLimit(DepositLimitInfo depositLimitInfo, string Operator)
        {
            return depositService.AddDepositLimitInfo(depositLimitInfo, Operator);
        }

        public bool EditDepositLimit(DepositLimitInfo depositLimitInfo, string Operator)
        {
            return depositService.EditDepositLimitInfo(depositLimitInfo, Operator);
        }

        public int DeleteDepositLimit(DepositLimitInfo depositLimitInfo, string Operator)
        {
            var currentUpperPoint = depositService.GetDepositLimitInfo(depositLimitInfo.SiteId, depositLimitInfo.DepositType, depositLimitInfo.CurrencyId, depositLimitInfo.VipLevel + 1);

            if (currentUpperPoint != null)
                return 0;

            return depositService.DeleteDepositLimitInfo(depositLimitInfo, Operator);
        }

        private readonly Dictionary<string, System.Drawing.Imaging.ImageFormat> _contentTypesCheck = new Dictionary<string, System.Drawing.Imaging.ImageFormat>(comparer: StringComparer.OrdinalIgnoreCase)
        {
            {"image/png",System.Drawing.Imaging.ImageFormat.Png},
            {"image/jpeg",System.Drawing.Imaging.ImageFormat.Jpeg},
            { "image/gif",System.Drawing.Imaging.ImageFormat.Gif}
        };

        public DepositTimeData DepositTimeReportGet(DepositTimeReportPara para)
        {
            var result = depositService.DepositTimeReportGet(para);

             result?.DepositTimeList.ForEach(x =>
            {
                var ts1 = new TimeSpan(x.CreateTime.Ticks);
                var ts2 = new TimeSpan(x.UpdateTime.Ticks);
                var ts = ts1.Subtract(ts2).Duration();
                x.CompleteTime = GetTimeString(ts);
            });

            return result;
        }

        public IEnumerable<DepositTimeExportData> DepositTimeExportGet(string selectSite, string depositId, DateTime startDate, DateTime endDate, int status, int currencyId)
        {
            var exportDataToExcel = new List<DepositTimeExportData>();
            var siteList = JsonConvert.DeserializeObject<IEnumerable<int>>(selectSite);

            var result = depositService.GetExportData(siteList, depositId, startDate, endDate, status, currencyId);

            result?.ForEach(x => {
                var ts1 = new TimeSpan(x.CreateTime.Ticks);
                var ts2 = new TimeSpan(x.UpdateTime.Ticks);
                var ts = ts1.Subtract(ts2).Duration();
                exportDataToExcel.Add(new DepositTimeExportData
                {
                    SiteName = x.SiteName,
                    DepositId = x.DepositId,
                    DepositType = x.DepositTypeName,
                    UserName = x.UserName,
                    Currency = x.IsoCode,
                    Status = x.StatusName,
                    CreateTime = x.CreateTime.DateTime,
                    UpdateTime = x.UpdateTime.DateTime,
                    CompleteTime = GetTimeString(ts)
                });
            });
            return exportDataToExcel;
        }
        private string GetTimeString(TimeSpan ts)
        {
            var tempDays = "";
            var tempHours = "";
            var tempMinutes = "";
            var tempSeconds = "";
            var result = "";
            if (ts.Days != 0)
            {
                tempDays = string.Format("{0}Days", ts.Days.ToString());
            }
            if (ts.Hours >= 10)
            {
                tempHours = string.Format("{0}", ts.Hours.ToString());
            }
            else
            {
                tempHours = string.Format("0{0}", ts.Hours.ToString());
            }
            if (ts.Minutes >= 10)
            {
                tempMinutes = string.Format("{0}", ts.Minutes.ToString());
            }
            else
            {
                tempMinutes = string.Format("0{0}", ts.Minutes.ToString());
            }
            if (ts.Seconds >= 10)
            {
                tempSeconds = string.Format("{0}", ts.Seconds.ToString());
            }
            else
            {
                tempSeconds = string.Format("0{0}", ts.Seconds.ToString());
            }
            result = string.Format("{0}{1}:{2}:{3}", tempDays, tempHours, tempMinutes, tempSeconds);
            return result;
        }


        public int SetDepositRequestAmount(string transId,decimal amount,string @operator,int custId)
        {
            var result = 1;

            using var scope = new TransactionScope();

            if(!depositService.SetDepositRequestAmount(transId, amount, @operator))
            {
                scope.Dispose();

                return 0;
            }
            if (!custInfoService.AddCustNote(custId, $"SetDepositRequestAmount:{amount}", @operator))
            {
                scope.Dispose();

                return 0;
            }

            scope.Complete();
            return result;
        }
    }
}

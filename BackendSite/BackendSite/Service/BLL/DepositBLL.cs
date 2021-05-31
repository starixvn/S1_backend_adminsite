using System;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using LazZiya.ImageResize;
using Microsoft.AspNetCore.Http;

namespace BackendSite.Service.BLL
{
    public class DepositBLL
    {
        private readonly DepositService depositService;
        private readonly ServerInfoService serverInfoService;
        private readonly AdjustmentService adjustmentService;
        private readonly CustInfoBLL custInfoBLL;
        private readonly AuthorizeBLL authorizeBLL;

        public DepositBLL(DepositService depositService, ServerInfoService serverInfoService, AdjustmentService adjustmentService, CustInfoBLL custInfoBLL, AuthorizeBLL authorizeBLL)
        {
            this.depositService = depositService;
            this.serverInfoService = serverInfoService;
            this.adjustmentService = adjustmentService;
            this.custInfoBLL = custInfoBLL;
            this.authorizeBLL = authorizeBLL;
        }
        public IEnumerable<DepositRequest> GetDepositRequestList(IEnumerable<int> selectSite, IEnumerable<int> depositType, long startDate, long endDate, string DepositId, string UserName, IEnumerable<int> currencyId, IEnumerable<int> status, IEnumerable<int> levelstatus,int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            var serverInfo = serverInfoService.GetModel();/*Check SiteID By Permission*/
            return depositService.GetDepositRequestList(selectSite, depositType, startDate, endDate, serverInfo.SiteId/*Not require*/, DepositId, UserName, currencyId, status, levelstatus,pageSize, ref pageNumber, out totalPages, out rowCount);
        }

        public IEnumerable<DepositRequest> GetDepositListByCust(int CustId, string TransId, int? Status, long startDate, long endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount,out decimal totalAmount)
        {
            return depositService.GetDepositListByCust(CustId, TransId, Status, startDate, endDate, pageSize, ref pageNumber, out totalPages, out rowCount,out totalAmount);
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
            deposit.DepositInfo = Newtonsoft.Json.JsonConvert.SerializeObject(depositInfo);

            var customer = custInfoBLL.GetCustInfoDetails(null, deposit.UserName, deposit.SiteId);
            if (customer == null)
            {
                return -1;
            }
            deposit.CurrencyId = customer.CurrencyId;
            deposit.CustId = customer.CustId;

            var creator = authorizeBLL.GetAuthData();
            deposit.Operator = creator.UserName;

            var transId = depositService.InsertDepositRequest(deposit);

            if (!string.IsNullOrEmpty(transId) && receipt!=null)
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
                decimal xSize = 500m;
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

                var graphicOptions = new GraphicOptions();
                graphicOptions.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                graphicOptions.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                var img = ImageResize.Scale(uploadedImage, Convert.ToInt32(uploadedImage.Width * xCell), Convert.ToInt32(uploadedImage.Height * xCell), graphicOptions);
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                string fileName = depositReceipt.FileName;
                fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
                fileName = $"{fileName.Substring(0, fileName.LastIndexOf('.'))}.jpg";

                byte[] fileBytes = ms.ToArray();

                processedDepositReceipt.FileName = fileName;
                processedDepositReceipt.FileBytes = fileBytes;
            }

            return processedDepositReceipt;
        }

        private int ValidateDepositReceiptPic(string transId)
        {
            int depositId = 0;
            Match m = Regex.Match(transId, @"\d+");
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
            int depositId = 0;

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out depositId);
            return depositService.GetDepositRequest(depositId, serverInfo.SiteId/*Not require*/);/*Check SiteID By Permission*/
        }

        public IEnumerable<Comments> GetDepositRequestComment(string transId)
        {
            var serverInfo = serverInfoService.GetModel();
            int depositId = 0;

            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out depositId);
            var depositRequest = GetDepositRequest(transId);
            return depositService.GetDepositRequestComment(depositId, depositRequest.SiteId);
        }

        public int DepositApproved(string transId, decimal actualDeposit, string comment, string Operator)
        {
            List<int> forStatus = new List<int>();
            forStatus.Add((int)DepositStatus.Pending);
            var depositRequest = GetDepositRequest(transId);

            return depositService.SetDepositInfo(transId, depositRequest.SiteId, depositRequest.Amount, comment, Operator, forStatus, DepositStatus.Approved, depositRequest);
        }

        public int DepositRejectd(string transId, decimal actualDeposit, string comment, string Operator)
        {
            List<int> forStatus = new List<int>();
            forStatus.Add((int)DepositStatus.Pending);

            var depositRequest = GetDepositRequest(transId);
            return depositService.SetDepositInfo(transId, depositRequest.SiteId, actualDeposit, comment, Operator, forStatus, DepositStatus.Rejected, null);
        }

        public int DepositDraft(string transId, decimal actualDeposit, string comment, string Operator)
        {
            List<int> forStatus = new List<int>();
            forStatus.Add((int)DepositStatus.Pending);
            forStatus.Add((int)DepositStatus.OnHold);

            var depositRequest = GetDepositRequest(transId);
            return depositService.SetDepositInfo(transId, depositRequest.SiteId, actualDeposit, comment, Operator, forStatus, DepositStatus.Draft, null);
        }
        public int DepositOnHold(string transId, decimal actualDeposit, string comment, string Operator)
        {
            List<int> forStatus = new List<int>();
            forStatus.Add((int)DepositStatus.Pending);
            var depositRequest = GetDepositRequest(transId);
            return depositService.SetDepositInfo(transId, depositRequest.SiteId, actualDeposit, comment, Operator, forStatus, DepositStatus.OnHold, null);
        }
        public int DepositPending(string transId, decimal actualDeposit, string comment, string Operator)
        {
            List<int> forStatus = new List<int>();
            forStatus.Add((int)DepositStatus.OnHold);
            var depositRequest = GetDepositRequest(transId);
            return depositService.SetDepositInfo(transId, depositRequest.SiteId, actualDeposit, comment, Operator, forStatus, DepositStatus.Pending, null);
        }
        public DepositReceipt GetDepositReceipt(string transId)
        {
            int depositId = 0;
            Match m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out depositId);
            var depositRequest = GetDepositRequest(transId);
            return depositService.GetDepositReceipt(depositId, depositRequest.SiteId);
        }
        public IEnumerable<DepositInfoForExport> GetDepositExportList(string selectSite, string depositType, long startDate, long endDate, string DepositId, string UserName, string currencyId, string status,string LevelStatus)
        {
            return depositService.GetDepositExportList(selectSite, depositType, startDate, endDate, DepositId, UserName, currencyId, status, LevelStatus);
        }
        public IEnumerable<DepositLimitCurrencyInfo> GetDepositLimitCurrencyList(int siteId, int depositType) => depositService.GetDepositLimitCurrencyList(siteId, depositType);

        public IEnumerable<DepositLimitInfo> GetDepositLimitInfos(int siteId, int depositType, int currencyId) => depositService.GetDepositLimitInfos(siteId, depositType,currencyId);

        public bool AddDepositLimit(DepositLimitInfo depositLimitInfo)
        {
            //if (depositLimitInfo.VipLevel > 1)
            //{
            //    var currentMaxLevel = depositService.GetDepositLimitInfo(depositLimitInfo.SiteId, depositLimitInfo.DepositType, depositLimitInfo.CurrencyId, depositLimitInfo.VipLevel - 1);

            //    if (depositLimitInfo.DepositMin <= currentMaxLevel.DepositMin || depositLimitInfo.DepositMax <= currentMaxLevel.DepositMax)
            //        return false;
            //}
            //else
            //{
            //    if (depositLimitInfo.DepositMin <= 0 || depositLimitInfo.DepositMax <= 0)
            //        return false;
            //}

            return depositService.AddDepositLimitInfo(depositLimitInfo);
        }

        public bool EditDepositLimit(DepositLimitInfo depositLimitInfo)
        {
            //var currentBelowLevel = depositService.GetDepositLimitInfo(depositLimitInfo.SiteId, depositLimitInfo.DepositType, depositLimitInfo.CurrencyId, depositLimitInfo.VipLevel - 1);
            //var currentUpperLevel = depositService.GetDepositLimitInfo(depositLimitInfo.SiteId, depositLimitInfo.DepositType, depositLimitInfo.CurrencyId, depositLimitInfo.VipLevel + 1);

            //if (currentBelowLevel != null)
            //{
            //    if (depositLimitInfo.DepositMin <= currentBelowLevel.DepositMin || depositLimitInfo.DepositMax <= currentBelowLevel.DepositMax)
            //        return false;
            //}
            //else
            //{
            //    if (depositLimitInfo.DepositMin <= 0 || depositLimitInfo.DepositMax <= 0)
            //        return false;
            //}

            //if (currentUpperLevel != null)
            //{
            //    if (depositLimitInfo.DepositMin >= currentUpperLevel.DepositMin || depositLimitInfo.DepositMax >= currentUpperLevel.DepositMax)
            //        return false;
            //}

            return depositService.EditDepositLimitInfo(depositLimitInfo);
        }

        public int DeleteDepositLimit(DepositLimitInfo depositLimitInfo)
        {
            var currentUpperPoint = depositService.GetDepositLimitInfo(depositLimitInfo.SiteId, depositLimitInfo.DepositType, depositLimitInfo.CurrencyId, depositLimitInfo.VipLevel + 1);

            if (currentUpperPoint != null)
                return 0;

            return depositService.DeleteDepositLimitInfo(depositLimitInfo);
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

            if(result != null)
            {
                result.DepositTimeList.ForEach(x =>
                {
                    TimeSpan ts1 = new TimeSpan(x.CreateTime.Ticks);
                    TimeSpan ts2 = new TimeSpan(x.UpdateTime.Ticks);
                    TimeSpan ts = ts1.Subtract(ts2).Duration();
                    string tempDays = "";
                    string tempHours = "";
                    string tempMinutes = "";
                    string tempSeconds = "";
                    if (ts.Days != 0)
                    {
                        tempDays = string.Format("{0}Days", ts.Days.ToString());
                    }
                    if (ts.Hours != 0)
                    {
                        tempHours = string.Format("{0}Hours", ts.Hours.ToString());
                    }
                    if (ts.Minutes != 0)
                    {
                        tempMinutes = string.Format("{0}Minutes", ts.Minutes.ToString());
                    }
                    if (ts.Seconds != 0)
                    {
                        tempSeconds = string.Format("{0}Seconds", ts.Seconds.ToString());
                    }
                    x.CompleteTime = string.Format("{0}{1}{2}{3}", tempDays, tempHours, tempMinutes, tempSeconds);
                    //x.CompleteTime = string.Format("{0}d{1}h{2}m{3}s", ts.Days.ToString(), ts.Hours.ToString(), ts.Minutes.ToString(), ts.Seconds.ToString());
                });
            }

            return result;
        }

        //public IEnumerable<DrawPrizeExcelData> DepositTimeExportGet(string SelectSite, string StartDate, string EndDate, int CurrencyId, string EventName)
        //{
        //    //var siteList = JsonConvert.DeserializeObject<IEnumerable<int>>(SelectSite);
        //    //return prizeService.GetExportData(siteList, StartDate, EndDate, CurrencyId, EventName);
        //}
    }
}

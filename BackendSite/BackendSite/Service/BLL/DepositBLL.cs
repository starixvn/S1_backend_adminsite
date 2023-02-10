using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using BackendSite.Service.Library;
using System.Transactions;
using BackendSite.Extensions;
using BackendSite.Controllers.ApiV2.Finance.Deposit;

namespace BackendSite.Service.BLL
{
    public class DepositBLL
    {
        private readonly DepositService depositService;
        private readonly ServerInfoService serverInfoService;
        private readonly DepositAccountingService _depositAccountingService;
        private readonly AdjustmentLogReportService _adjustmentLogReportService;
        private readonly CustInfoBLL custInfoBLL;
        private readonly AuthorizeBLL authorizeBLL;
        private readonly Common common;
        private readonly CustInfoService custInfoService;
        private readonly ImageEditorBLL _imageEditorBll;
        private readonly IConfiguration _configuration;
        private readonly CourierAdapter _courierAdapter;
        private readonly ICacheHelper _cacheHelper;
        private readonly string payApiUrl;
        private readonly ILogger<DepositBLL> _logger;


        public DepositBLL(IServiceProvider provider)
        {
            depositService = provider.GetRequiredService<DepositService>();
            serverInfoService = provider.GetRequiredService<ServerInfoService>();
            _depositAccountingService = provider.GetRequiredService<DepositAccountingService>();
            _adjustmentLogReportService = provider.GetRequiredService<AdjustmentLogReportService>();
            _depositAccountingService = provider.GetRequiredService<DepositAccountingService>();
            custInfoBLL = provider.GetRequiredService<CustInfoBLL>();
            authorizeBLL = provider.GetRequiredService<AuthorizeBLL>();
            common = provider.GetRequiredService<Common>();
            custInfoService = provider.GetRequiredService<CustInfoService>();
            _configuration = provider.GetRequiredService<IConfiguration>();
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
            payApiUrl = _configuration.GetValue<string>("PayService"); //DepositManual
            _imageEditorBll = provider.GetRequiredService<ImageEditorBLL>();
            _cacheHelper = provider.GetRequiredService<ICacheHelper>();
            _logger = provider.GetRequiredService<ILogger<DepositBLL>>();
            _depositAccountingService = provider.GetRequiredService<DepositAccountingService>();
        }

        public IEnumerable<DepositRequest> GetDepositRequestList(IEnumerable<int> selectSite, IEnumerable<int> depositType, IEnumerable<string> selectBankCode,
            IEnumerable<int> selectCalCode,
            DateTime startDate,
            DateTime endDate,
            string depositId,
            IEnumerable<string> userNames,
            string agent,
            IEnumerable<int> currencyId, 
            IEnumerable<int> status,
            IEnumerable<int> levelStatus, 
            int pageSize,
            decimal startAmount, 
            decimal endAmount,
            IEnumerable<int> payIds,
            int pageNumber, out int totalPages, out int rowCount, ref List<TotalAmount> currencyAmountRecord)
        {
            return depositService.GetDepositRequestList(selectSite, depositType, selectBankCode, selectCalCode, startDate, endDate, depositId, userNames,agent, currencyId, status, levelStatus, pageSize, startAmount, endAmount, payIds, pageNumber, out totalPages, out rowCount, ref currencyAmountRecord);
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

        public async Task<int> CreateDeposit(DepositDetailInfo deposit, IFormFile receipt)
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
                await using var fileStream = receipt.OpenReadStream();
                using var uploadedImage = await SixLabors.ImageSharp.Image.LoadAsync(fileStream);
                var fileBytes = await _imageEditorBll.Resize(uploadedImage);
                var fileName = receipt.FileName;
                fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
                fileName = $"{fileName.Substring(0, fileName.LastIndexOf('.'))}.jpg";

                await InsertDepositReceipt(deposit.CustId, transId, 1, fileName, fileBytes);
            }

            if (!string.IsNullOrEmpty(transId))
            {
                return 1;
            }
            return 0;
        }

        public async Task<int> DelDepositReceiptPic(int custId, string transId ,int receiptNo)
        {
            var depositId = ValidateDepositReceiptPic(transId);
            return await depositService.DelDepositReceiptPic(custId, depositId ,receiptNo);
        }
        
        public async Task<int> InsertDepositReceipt(int custId, string transId, int receiptNo , string fileName, byte[] fileBytes)
        {
            var depositId = ValidateDepositReceiptPic(transId);
            return await depositService.SetDepositReceiptPic(custId, depositId, receiptNo ,fileName, fileBytes);
        }

        public int CheckDepositReceipt(string transId,  string Operator)
        {
            var depositId = ValidateDepositReceiptPic(transId);
            return depositService.CheckDepositReceipt(depositId, Operator);
        }
        
        private int ValidateDepositReceiptPic(string transId)
        {
            var depositId = 0;
            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out depositId);
            return depositId;
        }

        public DepositRequest GetDepositRequest(string transId)
        {
            var serverInfo = serverInfoService.GetModel();

            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out var depositId);
            return depositService.GetDepositRequest(depositId, serverInfo.SiteId/*Not require*/);/*Check SiteID By Permission*/
        }

        public GetDepositAccountingListModel GetDepositAccountingList(
            IEnumerable<int> siteIds,
            IEnumerable<int> currencyIds,
            DateTime startDate,
            DateTime endDate,
            string transId,
            string userName,
            int pageNumber,
            int pageSize)
        {
            var result = _depositAccountingService.GetDepositAccountingList(
                JsonConvert.SerializeObject(siteIds),
                JsonConvert.SerializeObject(currencyIds),
                startDate,
                endDate,
                string.IsNullOrEmpty(transId) ? null : transId,
                string.IsNullOrEmpty(userName) ? null : userName,
                Math.Abs(pageSize),
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                out var totalPages,
                out var rowCount);

            return new GetDepositAccountingListModel()
            {
                Datas = result.ToList(),
                PageNumber = Math.Abs(pageNumber),
                RowCount = rowCount,
                TotalPages = totalPages
            };
        }

        public DepositAccountingModel GetDepositAccountingByTransId(string transId)
        {
            return _depositAccountingService.GetDepositAccountingByTransId(transId);
        }

        public int EditDepositAccounting(ParamsWithLog<DepositAccountingController.EditRequest> input)
        {
            return _depositAccountingService.EditDepositAccounting(input);
        }

        public IEnumerable<DepositAccountingModel> ExportDepositAccountingList(
            string siteIds,
            string currencyIds,
            DateTime startDate,
            DateTime endDate,
            string transId,
            string userName)
        {
            var result = _depositAccountingService.ExportDepositAccountingList(
                siteIds,
                currencyIds,
                startDate,
                endDate,
                string.IsNullOrEmpty(transId) ? null : transId,
                string.IsNullOrEmpty(userName) ? null : userName);

            return result;
        }

        public IEnumerable<DepositComments> GetDepositRequestComment(string transId)
        {
            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out var depositId);
            var depositRequest = GetDepositRequest(transId);
            return depositService.GetDepositRequestComment(depositId, depositRequest.SiteId);
        }

        public int DepositApproved(string transId, decimal actualDeposit, string comment, string Operator)
        {
            var forStatus = new List<int> { (int)DepositStatusEnum.Pending };
            var depositRequest = GetDepositRequest(transId);
            var result = depositService.SetDepositInfo(transId,
                depositRequest.SiteId,
                depositRequest.Amount,
                comment,
                Operator,
                forStatus,
                DepositStatusEnum.Approved,
                depositRequest,
                null,
                CommonExtension.CanRewardPoint(depositRequest.SiteId, depositRequest.CurrencyId));

            return result;
        }

        public int DepositReject(string transId, decimal actualDeposit, string comment, string Operator)
        {
            var forStatus = new List<int> { (int)DepositStatusEnum.Pending };

            var depositRequest = GetDepositRequest(transId);
            return depositService.SetDepositInfo(transId, depositRequest.SiteId, actualDeposit, comment, Operator, forStatus, DepositStatusEnum.Rejected, null ,null);
        }

        public int DepositDraft(string transId, decimal actualDeposit, string comment, string Operator, DepositDraftSetRequest draftInfo)
        {
            var forStatus = new List<int>
            {
                (int) DepositStatusEnum.Draft,
                (int) DepositStatusEnum.Pending,
                (int) DepositStatusEnum.Paid,
                (int) DepositStatusEnum.Approved,
                (int) DepositStatusEnum.Rejected,
                (int) DepositStatusEnum.Canceled,
                (int) DepositStatusEnum.Expired,
                (int) DepositStatusEnum.OnHold,
                (int) DepositStatusEnum.Error
            };

            var depositRequest = GetDepositRequest(transId);
            return depositService.SetDepositInfo(transId, depositRequest.SiteId, actualDeposit, comment, Operator, forStatus, DepositStatusEnum.Draft, null , draftInfo);
        }
        public int DepositOnHold(string transId, decimal actualDeposit, string comment, string Operator)
        {
            var forStatus = new List<int> {(int)DepositStatusEnum.Pending};
            var depositRequest = GetDepositRequest(transId);
            return depositService.SetDepositInfo(transId, depositRequest.SiteId, actualDeposit, comment, Operator, forStatus, DepositStatusEnum.OnHold, null , null);
        }
        public int DepositPending(string transId, decimal actualDeposit, string comment, string Operator)
        {
            var forStatus = new List<int> {(int)DepositStatusEnum.OnHold};
            var depositRequest = GetDepositRequest(transId);
            return depositService.SetDepositInfo(transId, depositRequest.SiteId, actualDeposit, comment, Operator, forStatus, DepositStatusEnum.Pending, null ,null);
        }
        public IEnumerable<DepositReceipt> GetDepositReceipt(string transId)
        {
            var m = Regex.Match(transId, @"\d+");
            int.TryParse(m.Value, out var depositId);
            var depositRequest = GetDepositRequest(transId);
            return depositService.GetDepositReceipt(depositId, depositRequest.SiteId);
        }
        public IEnumerable<DepositInfoForExport> GetDepositExportList(string selectSite,string selectBankCode,string selectCalCode ,string depositType, DateTime startDate, DateTime endDate, string depositId, IEnumerable<string> userNames,string agent, string currencyId, string status, string levelStatus ,decimal startAmount, decimal endAmount,string payIds)
        {
            return depositService.GetDepositExportList(selectSite, selectBankCode, selectCalCode, depositType, startDate, endDate, depositId, userNames, agent, currencyId, status, levelStatus, startAmount, endAmount, payIds);
        }
        public IEnumerable<DepositLimitCurrencyInfo> GetDepositLimitCurrencyList(int siteId, int depositType) => depositService.GetDepositLimitCurrencyList(siteId, depositType);

        public IEnumerable<DepositLimitInfo> GetDepositLimitInfos(IEnumerable<int> siteId, IEnumerable<int> depositType, IEnumerable<int> currencyId, int pageNumber, int pageSize, out int totalPages, out int rowCount)
            => depositService.GetDepositLimitInfos(JsonConvert.SerializeObject(siteId),
                JsonConvert.SerializeObject(depositType),
                JsonConvert.SerializeObject(currencyId),
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize),
                out totalPages,
                out rowCount);

        public IEnumerable<DepositLimitInfo> GetDepositLimitInfos(int siteId, int currencyId)
            => depositService.GetDepositLimitInfos(siteId, currencyId);

        public bool AddDepositLimit(ParamsWithLog<DepositLimitModel> input)
        {
            return depositService.AddDepositLimitInfo(input);
        }

        public bool AddDepositLimit(ParamsWithLog<IEnumerable<DepositLimitModel>> input)
        {
            return depositService.AddDepositLimitInfo(input);
        }

        public bool EditDepositLimit(ParamsWithLog<DepositLimitModel> input)
        {
            return depositService.EditDepositLimitInfo(input);
        }

        public bool EditDepositLimit(ParamsWithLog<IEnumerable<DepositLimitModel>> input)
        {
            var result = depositService.EditDepositLimitInfo(input);

            if (result)
            {
                _cacheHelper.KeyDelete($"DepositLimitInfoList:{input.SiteId}", RedisDataBase.Member);
            }

            return result;
        }

        public DepositLimitInfo GetDepositLimitInfo(int siteId, int depositType, int currencyId, int vipLevel)
        {
            var currentUpperPoint = depositService.GetDepositLimitInfo(siteId, depositType, currencyId, vipLevel);
            return currentUpperPoint;
        }

        public bool DeleteDepositLimit(ParamsWithLog<DepositLimitModel> input)
        {
            var currentUpperPoint =
                depositService.GetDepositLimitInfo(input.SiteId, (int)input.Before.DepositType, input.CurrencyId, input.Before.VipLevel + 1);

            if (currentUpperPoint != null)
                return false;

            return depositService.DeleteDepositLimitInfo(input);
        }

        public bool DeleteDepositLimitV2(ParamsWithLog<DepositLimitModel> input)
        {
            var result = depositService.DeleteDepositLimitInfoV2(input);

            if (result)
            {
                _cacheHelper.KeyDelete($"DepositLimitInfoList:{input.SiteId}", RedisDataBase.Member);
            }

            return result;
        }

        public IEnumerable<DepositTimeList> DepositTimeReportGet(DepositTimeReportPara para,
            out int totalPages, out int rowCount, out List<TotalAmount> currencyTotalAmount)
        {
            var result = depositService.DepositTimeReportGet(JsonConvert.SerializeObject(para.SiteIds),
                JsonConvert.SerializeObject(para.CurrencyId),
                JsonConvert.SerializeObject(para.Status),
                para.StartDate.DateTime,
                para.EndDate.DateTime,
                para.DepositId,
                para.CustId,
                (Math.Abs(para.PageNumber) - 1) * Math.Abs(para.PageSize),
                Math.Abs(para.PageSize),
                out totalPages,
                out rowCount,
                out currencyTotalAmount);

            if (result.Any())
            {
                result.ToList().ForEach(x =>
                {
                    var tsC = new TimeSpan(x.CreateTime.Ticks);
                    var tsU = new TimeSpan(x.UpdateTime.Ticks);
                    var ts = tsC.Subtract(tsU).Duration();
                    x.CompleteTime = GetTimeString(ts);

                    if (x.ApprovedTime > DateTime.Now.AddYears(-1000))
                    {
                        var tsA = new TimeSpan(x.ApprovedTime.Value.Ticks);
                        var ts2 = tsC.Subtract(tsA).Duration();
                        x.ToApprovedTime = GetTimeString(ts2);
                    }
                });
            }

            return result;
        }

        public async Task<IEnumerable<DepositTimeExportData>> DepositTimeExportGet(string selectSite, string depositId, DateTime startDate, DateTime endDate, string statuses, string currencyIds, string memberId)
        {
            var exportDataToExcel = new List<DepositTimeExportData>();
            int custId = 0;
            if (!string.IsNullOrWhiteSpace(memberId))
            {
                var custSimpleInfo = await custInfoBLL.GetCustId(null, memberId, JsonConvert.DeserializeObject<List<int>>(selectSite).FirstOrDefault());
                if (custSimpleInfo != null)
                    custId = custSimpleInfo.CustId;
                else
                    return exportDataToExcel;
            }

            var result = depositService.GetExportData(selectSite, depositId, startDate, endDate, statuses, currencyIds, custId);

            result?.ForEach(x =>
            {
                var ts1 = new TimeSpan(x.CreateTime.Ticks);
                var ts2 = new TimeSpan(x.UpdateTime.Ticks);
                var ts = ts1.Subtract(ts2).Duration();

                if (x.ApprovedTime > DateTime.Now.AddYears(-1000))
                {
                    var tsA = new TimeSpan(x.ApprovedTime.Value.Ticks);
                    var ts3 = ts1.Subtract(tsA).Duration();
                    x.ToApprovedTime = GetTimeString(ts3);
                }

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
                    CompleteTime = GetTimeString(ts),
                    ToApprovedTime = x.ToApprovedTime,
                    EndFlowTime = x.ApprovedTime,
                    ApprovedBy = x.ApprovedBy,
                    StatementTime = x.StatementTime,
                    Operator = x.Operator,
                    SendMessage = string.IsNullOrEmpty(x.DepositMessage) ? string.Empty : GetSendMessage(JsonConvert.DeserializeObject<int[]>(x.DepositMessage))
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


        public int SetDepositRequestAmount(ParamsWithLog<SetAmountRequest> input, int custId)
        {
            var result = 1;
            using var scope = new TransactionScope();

            if (!depositService.SetDepositRequestAmount(input))
            {
                scope.Dispose();

                return 0;
            }

            var logParam = new CreateAdminAdjustmentInfoLogInput
            {
                Operator = input.Operator,
                SiteId = input.SiteId,
                CurrencyId = input.CurrencyId,
                RefTransId = input.TransId,
                ActionType = ActionType.Update,
                MenuEnum = AdminAdjustmentLogMenuEnum.SetDepositRequestAmount,
                BeforeDetailInfo = JsonConvert.SerializeObject(input.Before),
                AfterDetailInfo = JsonConvert.SerializeObject(input.After)
            };

            if (!_adjustmentLogReportService.CreateAdminAdjustmentInfoLog(logParam))
            {
                scope.Dispose();

                return 0;
            }

            if (!custInfoService.AddCustNote(custId, $"SetDepositRequestAmount:{input.After.Amount}", input.Operator))
            {
                scope.Dispose();

                return 0;
            }

            scope.Complete();
            return result;
        }

        public async Task<ApiResult<DepositManualResponse>> DepositManual(DepositManualRequest depositManualRequest)
        {
            var url = $"{payApiUrl}DepositManual";
            var response = await _courierAdapter.MyPostAsync<DepositManualResponse, DepositManualRequest>(url, depositManualRequest);
            return response;
        }

        /// <summary>
        /// 取得 paymentGateWay
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PaymentGateWayOptions> GetPaymentGateWayOptions()
        {
            return depositService.GetPaymentGateWayOptions();
        }

        public int SetDepositMessage(ParamsWithLog<DepositMessageRequest> input)
        {
            var result = 1;

            if (!depositService.SetDepositMessage(input))
            {
                return 0;
            }

            var logParam = new CreateAdminAdjustmentInfoLogInput
            {
                Operator = input.Operator,
                SiteId = input.SiteId,
                CurrencyId = input.CurrencyId,
                RefTransId = input.TransId,
                ActionType = ActionType.Create,
                MenuEnum = AdminAdjustmentLogMenuEnum.SetDepositMessage,
                BeforeDetailInfo = null,
                AfterDetailInfo = JsonConvert.SerializeObject(input.After)
            };

            _adjustmentLogReportService.CreateAdminAdjustmentInfoLog(logParam);
            depositService.SetDepositLog(input.After.DepositId, input.SiteId, $"SendMessage:{input.After.StatusName}", (int)input.After.Action, input.After.ActualDeposit, input.Operator);

            return result;
        }

        private string GetSendMessage(int[] depositMessages)
        {
            List<string> depositMessageList = new List<string>();
            depositMessages.ToList().ForEach(x =>
            {
                depositMessageList.Add(((DepositMessageStatus)x).ToString());
            });
            return String.Join(", ", depositMessageList.ToArray());
        }
    }
}
